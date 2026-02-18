namespace CommerceHub.NotificationService.Domain;

/// <summary>
/// Business kind of notification (order placed, shipped, etc.).
/// </summary>
public enum NotificationKind
{
    OrderPlaced,
    OrderPaid,
    OrderShipped,
    OrderCancelled
}
