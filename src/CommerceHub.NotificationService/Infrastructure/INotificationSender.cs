using CommerceHub.NotificationService.Domain;

namespace CommerceHub.NotificationService.Infrastructure;

/// <summary>
/// Abstraction for sending notifications (email, SMS, push).
/// </summary>
public interface INotificationSender
{
    Task SendAsync(NotificationJob job, CancellationToken ct = default);
}
