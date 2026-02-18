using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Messaging;
using CommerceHub.Contracts.Order;
using CommerceHub.OrderingService.Domain;
using FluentValidation;
using Marten;
using Marten.Events.Projections;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Marten;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();

var connectionString = configuration.GetConnectionString("orderingDb") ?? "Host=localhost;Port=5432;Database=commercehub_ordering;Username=commercehub;Password=changeme";

builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    opts.Projections.Snapshot<Order>(SnapshotLifecycle.Inline);
    opts.Events.UseIdentityMapForAggregates = true;
})
.IntegrateWithWolverine(o => o.UseFastEventForwarding = true);

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq") ?? "amqp://localhost"))
        .AutoProvision();
    opts.ListenToRabbitQueue(QueueNames.OrderCommands);
    opts.PublishMessage<OrderPlacedV1>().ToRabbitQueue(QueueNames.OrderEvents);
    opts.PublishMessage<OrderPaidV1>().ToRabbitQueue(QueueNames.OrderEvents);
    opts.PublishMessage<OrderCancelledV1>().ToRabbitQueue(QueueNames.OrderEvents);
    opts.PublishMessage<OrderShippedV1>().ToRabbitQueue(QueueNames.OrderEvents);
    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();
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

await app.RunAsync();
