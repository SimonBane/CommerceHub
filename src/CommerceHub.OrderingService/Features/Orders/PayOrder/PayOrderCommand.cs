namespace CommerceHub.OrderingService.Features.Orders.PayOrder;

public sealed record PayOrderCommand(Guid PaymentId, decimal Amount, string CurrencyCode = "USD");
