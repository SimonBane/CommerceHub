namespace CommerceHub.Contracts.Payment;

/// <summary>
/// Published when payment authorization fails.
/// Consumed by CheckoutOrchestrator to abort checkout and release inventory.
/// </summary>
public sealed record PaymentFailedV1(
    Guid OrderId,
    string Reason,
    DateTime FailedAt);
