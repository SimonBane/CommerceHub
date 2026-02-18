namespace CommerceHub.Contracts.Basket;

/// <summary>
/// Published when a customer initiates checkout from their basket.
/// Consumed by CheckoutOrchestrator to start the checkout saga.
/// </summary>
public sealed record CheckoutInitiatedV1(
    Guid BasketId,
    Guid CustomerId,
    IReadOnlyList<BasketItemDto> Items,
    decimal Total,
    DateTime InitiatedAt);

public sealed record BasketItemDto(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity);
