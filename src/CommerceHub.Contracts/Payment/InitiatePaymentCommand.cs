namespace CommerceHub.Contracts.Payment;

/// <summary>
/// Command sent by CheckoutOrchestrator to initiate payment for an order.
/// </summary>
public sealed record InitiatePaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    string? IdempotencyKey = null);
