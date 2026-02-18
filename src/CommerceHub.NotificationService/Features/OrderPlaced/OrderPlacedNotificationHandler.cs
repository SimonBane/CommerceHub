using CommerceHub.Contracts.Order;
using CommerceHub.NotificationService.Constants;
using CommerceHub.NotificationService.Domain;
using CommerceHub.NotificationService.Infrastructure;
using Marten;
using Wolverine;

namespace CommerceHub.NotificationService.Features.OrderPlaced;

/// <summary>
/// Handles OrderPlacedV1 from order-events. Creates NotificationJob and sends via outbox pattern.
/// </summary>
public static class OrderPlacedNotificationHandler
{
    public static async Task Handle(
        OrderPlacedV1 evt,
        IDocumentSession session,
        INotificationSender sender,
        CancellationToken ct)
    {
        var existing = await session.Query<NotificationJob>()
            .Where(j => j.CorrelationId == evt.OrderId.ToString() && j.Kind == NotificationKind.OrderPlaced)
            .AnyAsync(ct);

        if (existing)
            return;

        var job = NotificationJob.Create(
            evt.CustomerId,
            NotificationType.Email,
            NotificationKind.OrderPlaced,
            string.Format(NotificationTemplates.OrderPlaced.Subject, evt.OrderId.ToString("N")[..8]),
            string.Format(NotificationTemplates.OrderPlaced.Body, evt.OrderId.ToString("N")[..8], evt.Total, evt.CurrencyCode),
            evt.OrderId.ToString());

        session.Store(job);
        await session.SaveChangesAsync(ct);

        await sender.SendAsync(job, ct);

        job.MarkAsSent();
        await session.SaveChangesAsync(ct);
    }
}
