namespace CommerceHub.PaymentService.Infrastructure.PaymentGateway;

public sealed record PaymentGatewayRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode);
