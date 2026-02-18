using CommerceHub.BuildingBlocks;
using CommerceHub.Contracts.Messaging;
using CommerceHub.NotificationService.Constants;
using CommerceHub.NotificationService.Infrastructure;
using Marten;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString(ConfigurationKeys.NotificationDb)!;
builder.Services.AddMarten(opts => opts.Connection(connectionString))
    .IntegrateWithWolverine();

builder.Services.AddScoped<INotificationSender, SimulatedNotificationSender>();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();

    opts.UseRabbitMq(new Uri(configuration.GetConnectionString("rabbitMq")!))
        .AutoProvision();

    opts.ListenToRabbitQueue(QueueNames.OrderEvents);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

await app.RunAsync();
