using Riok.Mapperly.Abstractions;

namespace Test.Api.Data.Mapping;

public static partial class TodosMapper
{
    public static IQueryable<Todo> ProjectToTodo(this IQueryable<TodoEntity> q)
    {
        return q.Select(e => new Todo(e.Id, e.Title, e.DueBy, e.IsComplete));
    }
}