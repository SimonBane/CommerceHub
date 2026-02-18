namespace CommerceHub.PaymentService.Infrastructure.PaymentGateway;

public sealed record PaymentGatewayResult(bool Success, string? TransactionId, string? FailureReason);
