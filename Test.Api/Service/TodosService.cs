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
            async (_) => await GetTodoFromDatabase(id),
            default,
            (FusionCacheEntryOptions)null!,
            cancellationToken
        );
    }

    public async Task<List<Todo>> GetTodosAsync(PagingGet getRequest, CancellationToken cancellationToken)
    {
        var pageSize = (int)getRequest.PageSize;
        var page = (int)getRequest.Page;
        return await _fusionCache.GetOrSetAsync(
            $"todos:page-{page}:size-{pageSize}",
            async (_) =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                await using var dbContext = scope.ServiceProvider.GetRequiredService<TodosContext>();
                var entities = await dbContext
                    .Set<TodoEntity>()
                    .OrderBy(e => e.CreatedAt)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToListAsync(CancellationToken.None);
                return TodosMapper.MapToTodo(entities);
            },
            default,
            (FusionCacheEntryOptions)null!,
            cancellationToken
        );
    }

    private async Task<Todo?> GetTodoFromDatabase(long id)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TodosContext>();
        var _id = id;
        var entity = await dbContext
            .Set<TodoEntity>()
            .FirstOrDefaultAsync(e => e.Id == _id);
        return TodosMapper.MapToTodo(entity);
    }
}