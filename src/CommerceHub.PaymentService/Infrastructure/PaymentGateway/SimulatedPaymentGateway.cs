namespace CommerceHub.PaymentService.Infrastructure.PaymentGateway;

/// <summary>
/// Simulated payment gateway for development and testing. Always succeeds unless configured to fail.
/// </summary>
public sealed class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly IConfiguration _configuration;

    public SimulatedPaymentGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<PaymentGatewayResult> AuthorizeAsync(PaymentGatewayRequest request, CancellationToken ct = default)
    {
        // For testing: fail if amount is 0 or negative
        if (request.Amount <= 0)
        {
            return Task.FromResult(new PaymentGatewayResult(false, null, "Invalid amount"));
        }

        // Optional: fail for specific order IDs (e.g. for testing failure scenarios)
        var failOrderIds = _configuration.GetSection("Payment:SimulateFailOrderIds").Get<Guid[]>();
        if (failOrderIds?.Contains(request.OrderId) == true)
        {
            return Task.FromResult(new PaymentGatewayResult(false, null, "Simulated payment failure"));
        }

        var transactionId = $"sim_{Guid.NewGuid():N}"[..24];
        return Task.FromResult(new PaymentGatewayResult(true, transactionId, null));
    }
}
