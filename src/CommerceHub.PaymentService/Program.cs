using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Payment;
using CommerceHub.Contracts.Messaging;
using CommerceHub.PaymentService.Infrastructure.PaymentGateway;
using CommerceHub.PaymentService.Infrastructure.Persistence;
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

builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();

var connectionString = configuration.GetConnectionString("paymentDb")
    ?? "Host=localhost;Port=5432;Database=commercehub_payment;Username=commercehub;Password=changeme";

builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(connectionString);
    opts.UseEntityFrameworkCoreTransactions();
    opts.Services.AddDbContextWithWolverineIntegration<PaymentDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();

    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq") ?? "amqp://localhost"))
        .AutoProvision();

    opts.PublishMessage<PaymentAuthorizedV1>().ToRabbitQueue(QueueNames.PaymentEvents);
    opts.PublishMessage<PaymentFailedV1>().ToRabbitQueue(QueueNames.PaymentEvents);
    opts.ListenToRabbitQueue(QueueNames.PaymentCommands);
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
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
