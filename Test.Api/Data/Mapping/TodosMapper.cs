namespace Test.Api.Data.Mapping;

public static class TodosMapper
{
    public static Todo[] MapToTodo(this List<TodoEntity> entities)
    {
        return entities.Select(e => new Todo(e.Id, e.Title, e.DueBy, e.IsComplete)).ToArray();
    }
    public static Todo? MapToTodo(this TodoEntity? entity)
    {
        return entity != null ? new Todo(entity.Id, entity.Title, entity.DueBy, entity.IsComplete) : null;
    }
}