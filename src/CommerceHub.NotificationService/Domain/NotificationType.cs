namespace CommerceHub.NotificationService.Domain;

/// <summary>
/// Type of notification to send (email, SMS, push).
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    Push
}
