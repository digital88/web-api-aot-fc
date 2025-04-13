namespace Test.Api.Data;

public sealed record Todo(long Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

public sealed record CreateTodoDto(string Title, DateOnly? DueBy = null, bool IsComplete = false);
