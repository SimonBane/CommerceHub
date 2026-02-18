using CommerceHub.BackofficeApi.Constants;
using CommerceHub.BuildingBlocks;
using FluentValidation;
using MongoDB.Driver;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

var mongoConnectionString = configuration.GetConnectionString(ConfigurationKeys.MongoDb)
    ?? "mongodb://localhost:27017";
var databaseName = configuration["ConnectionStrings:MongoDatabaseName"] ?? "commercehub_search";

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = new MongoClient(mongoConnectionString);
    return client.GetDatabase(databaseName);
});

builder.Services.AddHttpClient("CatalogService", client =>
{
    client.BaseAddress = new Uri("http://commercehub-catalogservice/");
})
    .AddServiceDiscovery()
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient("OrderingService", client =>
{
    client.BaseAddress = new Uri("http://commercehub-orderingservice/");
})
    .AddServiceDiscovery()
    .AddStandardResilienceHandler();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();

builder.Host.UseWolverine(opts => opts.UseResiliencePolicies());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapWolverineEndpoints(opts => opts.UseFluentValidationProblemDetailMiddleware());

await app.RunAsync();
