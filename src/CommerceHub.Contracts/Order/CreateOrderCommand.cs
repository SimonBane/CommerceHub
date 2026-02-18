namespace CommerceHub.Contracts.Order;

/// <summary>
/// Command sent by CheckoutOrchestrator to create an order from basket checkout.
/// </summary>
public sealed record CreateOrderCommand(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    string CurrencyCode = "USD");
