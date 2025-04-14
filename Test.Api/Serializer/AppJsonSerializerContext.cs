using System.Text.Json.Serialization;
using Test.Api.Data;

namespace Test.Api.Serializer;

[JsonSerializable(typeof(List<Todo>))]
[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(CreateTodoDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}