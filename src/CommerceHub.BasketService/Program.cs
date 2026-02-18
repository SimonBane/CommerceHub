using CommerceHub.Contracts.Basket;
using CommerceHub.Contracts.Messaging;
using CommerceHub.BasketService.Infrastructure;
using CommerceHub.BasketService.Infrastructure.Abstractions;
using CommerceHub.BuildingBlocks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.RabbitMQ;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var redisConnectionString = configuration.GetConnectionString("redis")!;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();
builder.Services.AddSwaggerWithAuth(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = configuration["Authentication:ValidAudience"];
        options.MetadataAddress = configuration["Authentication:MetadataAddress"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = configuration["Authentication:ValidIssuer"]
        };
    });

builder.Services.AddFusionCache()
    .WithDistributedCache(sp =>
    {
        var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
        var options = new RedisCacheOptions { ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer) };
        return new RedisCache(Microsoft.Extensions.Options.Options.Create(options));
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer());

builder.Services.AddSingleton<IBasketStore, BasketStore>();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq") ?? "amqp://localhost"))
        .AutoProvision();
    opts.PublishMessage<CheckoutInitiatedV1>().ToRabbitQueue(QueueNames.BasketEvents);
    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});
app.UseAuthentication();
app.UseAuthorization();

app.Run();
