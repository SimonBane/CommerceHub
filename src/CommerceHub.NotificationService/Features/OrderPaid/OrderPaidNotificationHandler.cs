using CommerceHub.Contracts.Order;
using CommerceHub.NotificationService.Constants;
using CommerceHub.NotificationService.Domain;
using Marten;
using Wolverine;

namespace CommerceHub.NotificationService.Features.OrderPaid;

/// <summary>
/// Handles OrderPaidV1 from order-events. Creates NotificationJob and sends.
/// </summary>
public static class OrderPaidNotificationHandler
{
    public static async Task Handle(
        OrderPaidV1 evt,
        IDocumentSession session,
        Infrastructure.INotificationSender sender,
        CancellationToken ct)
    {
        var existing = await session.Query<NotificationJob>()
            .Where(j => j.CorrelationId == evt.OrderId.ToString() && j.Kind == NotificationKind.OrderPaid)
            .AnyAsync(ct);

        if (existing)
            return;

        var job = NotificationJob.Create(
            evt.CustomerId,
            NotificationType.Email,
            NotificationKind.OrderPaid,
            string.Format(NotificationTemplates.OrderPaid.Subject, evt.OrderId.ToString("N")[..8]),
            string.Format(NotificationTemplates.OrderPaid.Body, evt.OrderId.ToString("N")[..8], evt.Amount, evt.CurrencyCode),
            evt.OrderId.ToString());

        session.Store(job);
        await session.SaveChangesAsync(ct);

        await sender.SendAsync(job, ct);

        job.MarkAsSent();
        await session.SaveChangesAsync(ct);
    }
}
