
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Test.Api.Common;
using Test.Api.Data;
using Test.Api.Data.Validation;
using Test.Api.Migrations.EfModel;
using Test.Api.Models;
using Test.Api.Serializer;
using Test.Api.Service;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var connectionString = builder.Configuration.GetConnectionString("Default") ??
    throw new ConfigurationException("Connection string is not set.");

builder.Services.AddDbContext<TodosContext>(
    builder => builder
    .UseNpgsql(connectionString)
    .UseModel(TodosContextModel.Instance));
builder.Services.AddTransient<IValidator<CreateTodoDto>, CreateTodoValidatior>();
builder.Services.AddScoped<ITodosService, TodosService>();

var appsettingsKeyFc = "FusionCache";
//
// This bindings directly from appsettings to FusionCacheOptions / FusionCacheEntryOptions do not work in AOT.
//
// var fusionCacheOptions = builder.Configuration
//     .GetSection(appsettingsKeyFc)
//     .GetSection(nameof(FusionCacheOptions))
//     .Get<FusionCacheOptions>() ?? throw new ConfigurationException($"{nameof(FusionCacheOptions)} is not set."); ..
//
// var fusionCacheEntryOptions = builder.Configuration
//     .GetSection(appsettingsKeyFc)
//     .GetSection(nameof(FusionCacheEntryOptions))
//     .Get<FusionCacheEntryOptions>() ?? throw new ConfigurationException($"{nameof(FusionCacheEntryOptions)} is not set."); ..
//
var redisCacheConfigurationOptions = builder.Configuration
    .GetSection(appsettingsKeyFc)
    .GetSection(nameof(RedisCacheOptions))
    .Get<RedisCacheConfigurationOptions>() ?? throw new ConfigurationException($"{nameof(RedisCacheConfigurationOptions)} is not set.");
var redisBackplaneConfigurationOptions = builder.Configuration
    .GetSection(appsettingsKeyFc)
    .GetSection(nameof(RedisBackplaneOptions))
    .Get<RedisCacheConfigurationOptions>() ?? throw new ConfigurationException($"{nameof(RedisCacheConfigurationOptions)} is not set.");
var redisCacheOptions = new RedisCacheOptions
{
    ConfigurationOptions = new()
    {
        Ssl = redisCacheConfigurationOptions.Ssl,
        Password = redisCacheConfigurationOptions.Password,
        EndPoints = [.. redisCacheConfigurationOptions.GetDnsEndPoints()]
    }
};
var redisBackplaneOptions = new RedisBackplaneOptions
{
    ConfigurationOptions = new()
    {
        Ssl = redisBackplaneConfigurationOptions.Ssl,
        Password = redisBackplaneConfigurationOptions.Password,
        EndPoints = [.. redisBackplaneConfigurationOptions.GetDnsEndPoints()]
    }
};
builder.Services.AddMemoryCache();
builder.Services.AddFusionCache()
    .WithOptions(new FusionCacheOptions()
    {
        DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(10)
    })
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
    .WithDistributedCache(new RedisCache(redisCacheOptions))
    .WithBackplane(new RedisBackplane(redisBackplaneOptions));
var app = builder.Build();

app.MapHealthChecks("/healthz");

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
todosApi.MapGet("/{id:guid}", async Task<Results<Ok<Todo>, NotFound>> (
    [FromRoute] long id,
    ITodosService todosService,
    CancellationToken cancellationToken) =>
{
    var todo = await todosService.GetTodoAsync(id, cancellationToken);
    return todo == null ? TypedResults.NotFound() : TypedResults.Ok(todo);
});

await app.RunAsync();
