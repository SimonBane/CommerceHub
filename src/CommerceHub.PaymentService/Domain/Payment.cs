namespace CommerceHub.PaymentService.Domain;

/// <summary>
/// Payment record for an order. Persisted with EF Core.
/// </summary>
public sealed class Payment
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; } = "USD";
    public PaymentStatus Status { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AuthorizedAt { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, Guid customerId, decimal amount, string currencyCode)
    {
        Id = Guid.CreateVersion7();
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        CurrencyCode = currencyCode;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAuthorized(string? externalTransactionId)
    {
        Status = PaymentStatus.Authorized;
        ExternalTransactionId = externalTransactionId;
        AuthorizedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }
}
