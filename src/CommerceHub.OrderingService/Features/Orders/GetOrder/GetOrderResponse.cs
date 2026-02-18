namespace CommerceHub.OrderingService.Features.Orders.GetOrder;

public sealed record GetOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLineResponse> Lines,
    decimal Total,
    string CurrencyCode,
    string Status,
    DateTime PlacedAt,
    DateTime? PaidAt,
    DateTime? ShippedAt,
    DateTime? CancelledAt,
    string? TrackingNumber);
    
public sealed record OrderLineResponse(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity);