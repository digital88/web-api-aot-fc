using System.Text.Json.Serialization;
using Test.Api.Data;

namespace Test.Api.Serializer;

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}