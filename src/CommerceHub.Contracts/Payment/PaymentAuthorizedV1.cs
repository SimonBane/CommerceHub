namespace CommerceHub.Contracts.Payment;

/// <summary>
/// Published when payment is successfully authorized.
/// Consumed by CheckoutOrchestrator to proceed with order creation.
/// </summary>
public sealed record PaymentAuthorizedV1(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    string? ExternalTransactionId,
    DateTime AuthorizedAt);
