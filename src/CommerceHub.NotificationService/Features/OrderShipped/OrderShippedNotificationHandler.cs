using CommerceHub.Contracts.Order;
using CommerceHub.NotificationService.Constants;
using CommerceHub.NotificationService.Domain;
using Marten;
using Wolverine;

namespace CommerceHub.NotificationService.Features.OrderShipped;

/// <summary>
/// Handles OrderShippedV1 from order-events. Creates NotificationJob and sends.
/// </summary>
public static class OrderShippedNotificationHandler
{
    public static async Task Handle(
        OrderShippedV1 evt,
        IDocumentSession session,
        Infrastructure.INotificationSender sender,
        CancellationToken ct)
    {
        var existing = await session.Query<NotificationJob>()
            .Where(j => j.CorrelationId == evt.OrderId.ToString() && j.Kind == NotificationKind.OrderShipped)
            .AnyAsync(ct);

        if (existing)
            return;

        var job = NotificationJob.Create(
            evt.CustomerId,
            NotificationType.Email,
            NotificationKind.OrderShipped,
            string.Format(NotificationTemplates.OrderShipped.Subject, evt.OrderId.ToString("N")[..8]),
            string.Format(NotificationTemplates.OrderShipped.Body, evt.OrderId.ToString("N")[..8], evt.TrackingNumber),
            evt.OrderId.ToString());

        session.Store(job);
        await session.SaveChangesAsync(ct);

        await sender.SendAsync(job, ct);

        job.MarkAsSent();
        await session.SaveChangesAsync(ct);
    }
}
