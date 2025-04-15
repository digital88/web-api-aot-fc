
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Test.Api.Data;
using Test.Api.Data.Validation;
using Test.Api.Models;
using Test.Api.Serializer;
using Test.Api.Service;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(
    options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default)
);

builder.Services.AddDbContext<TodosContext>();
builder.Services.AddTransient<IValidator<CreateTodoDto>, CreateTodoValidatior>();
builder.Services.AddScoped<ITodosService, TodosService>();
builder.Services.AddHealthChecks();

// FUSION CACHE SETUP
var FusionCacheKey = "FusionCache";
var rcco = new RedisCacheConfigurationOptions();
builder.Configuration
    .GetSection(FusionCacheKey)
    .GetSection(nameof(RedisCacheOptions))
    .Bind(rcco);
var rccoBackplane = new RedisCacheConfigurationOptions();
builder.Configuration
    .GetSection(FusionCacheKey)
    .GetSection(nameof(RedisBackplaneOptions))
    .Bind(rccoBackplane);
builder.Services.AddMemoryCache();
builder.Services.AddFusionCache()
    .WithOptions(new FusionCacheOptions()
    {
        DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(10)
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDefaultEntryOptions(new FusionCacheEntryOptions()
    {
        Duration = TimeSpan.FromMinutes(10),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(2),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),
        FactorySoftTimeout = TimeSpan.FromSeconds(1),
        FactoryHardTimeout = TimeSpan.FromSeconds(5),
        DistributedCacheSoftTimeout = TimeSpan.FromSeconds(2),
        DistributedCacheHardTimeout = TimeSpan.FromSeconds(20),
        AllowBackgroundDistributedCacheOperations = true,
        JitterMaxDuration = TimeSpan.FromSeconds(2),
    })
    .WithDistributedCache(new RedisCache(rcco.AsRedisCacheOptions()))
    .WithBackplane(new RedisBackplane(rccoBackplane.AsRedisBackplaneOptions()));
// FUSION CACHE SETUP END

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/");

var todosApi = app.MapGroup("/todos");
todosApi.MapPost("/", async Task<Created<long>> (
    [FromBody] CreateTodoDto createTodoDto,
    IValidator<CreateTodoDto> validator,
    ITodosService todosService,
    CancellationToken cancellationToken) =>
{
    await validator.ValidateAndThrowAsync(createTodoDto, cancellationToken);
    var id = await todosService.CreateTodoAsync(createTodoDto, cancellationToken);
    return TypedResults.Created("/todos/{id}", id);
});
todosApi.MapGet("/", async Task<Ok<List<Todo>>> (
    [AsParameters] PagingGet getRequest,
    ITodosService todosService,
    CancellationToken cancellationToken) =>
{
    var todos = await todosService.GetTodosAsync(getRequest, cancellationToken);
    return TypedResults.Ok(todos);
});
todosApi.MapGet("/{id:long}", async Task<Results<Ok<Todo>, NotFound>> (
    [FromRoute] long id,
    ITodosService todosService,
    CancellationToken cancellationToken) =>
{
    var todo = await todosService.GetTodoAsync(id, cancellationToken);
    return todo == null ? TypedResults.NotFound() : TypedResults.Ok(todo);
});

await app.RunAsync();
