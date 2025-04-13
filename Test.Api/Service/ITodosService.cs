using Test.Api.Data;
using Test.Api.Models;

namespace Test.Api.Service;

public interface ITodosService
{
    Task<long> CreateTodoAsync(CreateTodoDto createTodoDto, CancellationToken cancellationToken);
    Task<Todo?> GetTodoAsync(long id, CancellationToken cancellationToken);
    Task<List<Todo>> GetTodosAsync(PagingGet getRequest, CancellationToken cancellationToken);
}