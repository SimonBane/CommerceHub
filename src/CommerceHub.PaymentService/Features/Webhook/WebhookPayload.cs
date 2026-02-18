namespace CommerceHub.PaymentService.Features.Webhook;

/// <summary>
/// Generic webhook payload for external payment providers. Real implementations would use provider-specific DTOs.
/// </summary>
public sealed record WebhookPayload(
    string Provider,
    string EventType,
    string? TransactionId,
    string? Status,
    string? Reason);
