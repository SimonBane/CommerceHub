using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Inventory;
using CommerceHub.Contracts.Messaging;
using CommerceHub.Contracts.Order;
using CommerceHub.Contracts.Payment;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("checkoutOrchestratorDb")!;
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(connectionString);
    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();

    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq") ?? "amqp://localhost"))
        .AutoProvision();

    opts.ListenToRabbitQueue(QueueNames.BasketEvents)
        .CircuitBreaker(cb => { cb.MinimumThreshold = 5; cb.PauseTime = TimeSpan.FromMinutes(1); cb.FailurePercentageThreshold = 25; });
    opts.ListenToRabbitQueue(QueueNames.InventoryEvents)
        .CircuitBreaker(cb => { cb.MinimumThreshold = 5; cb.PauseTime = TimeSpan.FromMinutes(1); cb.FailurePercentageThreshold = 25; });
    opts.ListenToRabbitQueue(QueueNames.PaymentEvents)
        .CircuitBreaker(cb => { cb.MinimumThreshold = 5; cb.PauseTime = TimeSpan.FromMinutes(1); cb.FailurePercentageThreshold = 25; });
    opts.ListenToRabbitQueue(QueueNames.OrderEvents)
        .CircuitBreaker(cb => { cb.MinimumThreshold = 5; cb.PauseTime = TimeSpan.FromMinutes(1); cb.FailurePercentageThreshold = 25; });

    opts.PublishMessage<ReserveInventoryCommand>().ToRabbitQueue(QueueNames.InventoryCommands);
    opts.PublishMessage<ReleaseInventoryCommand>().ToRabbitQueue(QueueNames.InventoryCommands);
    opts.PublishMessage<InitiatePaymentCommand>().ToRabbitQueue(QueueNames.PaymentCommands);
    opts.PublishMessage<CreateOrderCommand>().ToRabbitQueue(QueueNames.OrderCommands);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

await app.RunAsync();
