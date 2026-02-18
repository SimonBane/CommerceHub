namespace CommerceHub.Contracts.Order;

/// <summary>
/// Published when an order is placed. Consumed by NotificationService, Backoffice, etc.
/// </summary>
public sealed record OrderPlacedV1(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    string CurrencyCode,
    DateTime PlacedAt);

public sealed record OrderLineDto(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity);
