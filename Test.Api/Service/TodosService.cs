using Microsoft.EntityFrameworkCore;
using Test.Api.Data;
using Test.Api.Data.Mapping;
using Test.Api.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Test.Api.Service;

public sealed class TodosService : ITodosService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IFusionCache _fusionCache;

    public TodosService(IServiceScopeFactory serviceScopeFactory, IFusionCache fusionCache)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _fusionCache = fusionCache;
    }

    public async Task<long> CreateTodoAsync(CreateTodoDto createTodoDto, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TodosContext>();
        var e = new TodoEntity()
        {
            Title = createTodoDto.Title,
            DueBy = createTodoDto.DueBy,
            IsComplete = createTodoDto.IsComplete,
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Set<TodoEntity>().Add(e);
        await dbContext.SaveChangesAsync(cancellationToken);
        return e.Id;
    }

    public async Task<Todo?> GetTodoAsync(long id, CancellationToken cancellationToken)
    {
        return await _fusionCache.GetOrSetAsync(
            $"todo:id-{id}",
            async (ct) => await GetTodoFromDatabase(id, ct),
            default,
            (FusionCacheEntryOptions)null!,
            cancellationToken
        );
    }

    public async Task<List<Todo>> GetTodosAsync(PagingGet getRequest, CancellationToken cancellationToken)
    {
        return await _fusionCache.GetOrSetAsync(
            $"todos:page-{getRequest.Page}:size-{getRequest.PageSize}",
            async (ct) => await GetTodosFromDatabase(getRequest, ct),
            default,
            (FusionCacheEntryOptions)null!,
            cancellationToken
        );
    }

    private async Task<Todo?> GetTodoFromDatabase(long id, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TodosContext>();
        var items = await dbContext.Database
            .SqlQueryRaw<Todo>("select \"Id\",\"Title\",\"DueBy\",\"IsComplete\" from todos.todos where \"Id\" = {Id}", [id])
            .ToListAsync(cancellationToken);
        return items.FirstOrDefault();
    }

    private async Task<List<Todo>> GetTodosFromDatabase(PagingGet getRequest, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TodosContext>();
        return await dbContext.Set<TodoEntity>()
            .OrderBy(e => e.CreatedAt)
            .Skip((int)(getRequest.Page * getRequest.PageSize))
            .Take((int)getRequest.PageSize)
            .ProjectToTodo()
            .ToListAsync(cancellationToken);
    }
}