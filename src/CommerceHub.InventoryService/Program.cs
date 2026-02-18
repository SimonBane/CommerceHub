using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Inventory;
using CommerceHub.Contracts.Messaging;
using CommerceHub.InventoryService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();

var connectionString = configuration.GetConnectionString("inventoryDb")!;
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(connectionString);
    opts.UseEntityFrameworkCoreTransactions();
    opts.Services.AddDbContextWithWolverineIntegration<InventoryDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();

    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq")!))
        .AutoProvision();

    opts.PublishMessage<InventoryReservedV1>().ToRabbitQueue(QueueNames.InventoryEvents);
    opts.PublishMessage<InventoryReservationFailedV1>().ToRabbitQueue(QueueNames.InventoryEvents);
    opts.ListenToRabbitQueue(QueueNames.InventoryCommands);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
