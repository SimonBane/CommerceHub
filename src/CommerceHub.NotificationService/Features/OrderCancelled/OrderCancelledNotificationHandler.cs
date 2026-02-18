using CommerceHub.Contracts.Order;
using CommerceHub.NotificationService.Constants;
using CommerceHub.NotificationService.Domain;
using Marten;
using Wolverine;

namespace CommerceHub.NotificationService.Features.OrderCancelled;

/// <summary>
/// Handles OrderCancelledV1 from order-events. Creates NotificationJob and sends.
/// </summary>
public static class OrderCancelledNotificationHandler
{
    public static async Task Handle(
        OrderCancelledV1 evt,
        IDocumentSession session,
        Infrastructure.INotificationSender sender,
        CancellationToken ct)
    {
        var existing = await session.Query<NotificationJob>()
            .Where(j => j.CorrelationId == evt.OrderId.ToString() && j.Kind == NotificationKind.OrderCancelled)
            .AnyAsync(ct);

        if (existing)
            return;

        var job = NotificationJob.Create(
            evt.CustomerId,
            NotificationType.Email,
            NotificationKind.OrderCancelled,
            string.Format(NotificationTemplates.OrderCancelled.Subject, evt.OrderId.ToString("N")[..8]),
            string.Format(NotificationTemplates.OrderCancelled.Body, evt.OrderId.ToString("N")[..8], evt.Reason),
            evt.OrderId.ToString());

        session.Store(job);
        await session.SaveChangesAsync(ct);

        await sender.SendAsync(job, ct);

        job.MarkAsSent();
        await session.SaveChangesAsync(ct);
    }
}
