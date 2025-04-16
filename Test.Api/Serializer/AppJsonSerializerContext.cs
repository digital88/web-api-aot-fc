using System.Text.Json.Serialization;
using Test.Api.Data;
using ZiggyCreatures.Caching.Fusion.Internals.Distributed;

namespace Test.Api.Serializer;

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(CreateTodoDto))]
[JsonSerializable(typeof(FusionCacheDistributedEntry<Todo>))]
[JsonSerializable(typeof(FusionCacheDistributedEntry<Todo[]>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}