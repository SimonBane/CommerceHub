namespace CommerceHub.Contracts.Order;

/// <summary>
/// Published when an order is paid. Consumed by CheckoutOrchestrator, NotificationService, etc.
/// </summary>
public sealed record OrderPaidV1(
    Guid OrderId,
    Guid CustomerId,
    Guid PaymentId,
    decimal Amount,
    string CurrencyCode,
    DateTime PaidAt);
