namespace CommerceHub.OrderingService.Domain.Events;

public sealed record OrderPaid(
    Guid PaymentId,
    decimal Amount,
    string CurrencyCode);