namespace CommerceHub.PaymentService.Infrastructure.PaymentGateway;

/// <summary>
/// Abstraction over external payment providers (Stripe, etc.). For MVP, a simulated implementation is used.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> AuthorizeAsync(PaymentGatewayRequest request, CancellationToken ct = default);
}
