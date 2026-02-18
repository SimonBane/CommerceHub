namespace CommerceHub.BackofficeApi.Features.Orders.GetOrder;

public sealed record GetOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    string CurrencyCode,
    string Status,
    DateTime PlacedAt,
    DateTime? PaidAt,
    DateTime? ShippedAt,
    DateTime? CancelledAt,
    string? TrackingNumber);

public sealed record OrderLineDto(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity);
