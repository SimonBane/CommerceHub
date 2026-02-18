namespace CommerceHub.Contracts.Order;

/// <summary>
/// Published when an order is shipped. Consumed by NotificationService, etc.
/// </summary>
public sealed record OrderShippedV1(
    Guid OrderId,
    Guid CustomerId,
    string TrackingNumber,
    DateTime ShippedAt);
