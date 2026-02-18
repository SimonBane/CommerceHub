using CommerceHub.BuildingBlocks;
using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using CommerceHub.Contracts.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Wolverine;
using Wolverine.Http;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
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


builder.Services.AddWolverineHttp();

var connectionString = configuration.GetConnectionString("catalogDb");
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(connectionString!);
    opts.UseEntityFrameworkCoreTransactions();
    opts.Services.AddDbContextWithWolverineIntegration<CatalogDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();

    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq")!))
        .AutoProvision();

    opts.PublishMessage<ProductCreated>().ToRabbitQueue(QueueNames.CatalogEvents);
    opts.PublishMessage<ProductUpdated>().ToRabbitQueue(QueueNames.CatalogEvents);
    opts.PublishMessage<ProductDeleted>().ToRabbitQueue(QueueNames.CatalogEvents);
    opts.PublishMessage<CategoryCreated>().ToRabbitQueue(QueueNames.CatalogEvents);
    opts.PublishMessage<CategoryUpdated>().ToRabbitQueue(QueueNames.CatalogEvents);
    opts.PublishMessage<CategoryDeleted>().ToRabbitQueue(QueueNames.CatalogEvents);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
    opts.RequireAuthorizeOnAll();
});

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
