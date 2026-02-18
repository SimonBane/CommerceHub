using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoConnectionString = configuration.GetConnectionString("mongoDb") ?? "mongodb://localhost:27017";
    var databaseName = configuration["ConnectionStrings:MongoDatabaseName"] ?? "commercehub_search";
    var client = new MongoClient(mongoConnectionString);
    return client.GetDatabase(databaseName);
});

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

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq") ?? "amqp://localhost"))
        .AutoProvision()
        .DeclareQueue(QueueNames.CatalogEvents);
    opts.ListenToRabbitQueue(QueueNames.CatalogEvents);
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
    opts.RequireAuthorizeOnAll();
});
app.UseAuthentication();
app.UseAuthorization();

app.Run();
