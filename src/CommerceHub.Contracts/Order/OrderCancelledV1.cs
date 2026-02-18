namespace CommerceHub.Contracts.Order;

/// <summary>
/// Published when an order is canceled.
/// </summary>
public sealed record OrderCancelledV1(
    Guid OrderId,
    Guid CustomerId,
    string Reason,
    DateTime CancelledAt);
