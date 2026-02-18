using CommerceHub.NotificationService.Domain;
using Microsoft.Extensions.Logging;

namespace CommerceHub.NotificationService.Infrastructure;

/// <summary>
/// Simulated notification sender for development and testing. Logs instead of sending.
/// </summary>
public sealed class SimulatedNotificationSender(ILogger<SimulatedNotificationSender> logger) : INotificationSender
{
    public Task SendAsync(NotificationJob job, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[Simulated] Sending {Type} notification ({Kind}) to recipient {RecipientId}: {Subject}",
            job.Type,
            job.Kind,
            job.RecipientId,
            job.Subject);

        return Task.CompletedTask;
    }
}
