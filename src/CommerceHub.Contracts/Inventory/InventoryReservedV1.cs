namespace CommerceHub.Contracts.Inventory;

/// <summary>
/// Published when inventory is successfully reserved for an order.
/// Consumed by CheckoutOrchestrator to proceed with payment.
/// </summary>
public sealed record InventoryReservedV1(
    Guid ReservationId,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<ReservedItemDto> Items,
    DateTime ReservedAt);

public sealed record ReservedItemDto(
    Guid ProductId,
    string Sku,
    int Quantity);
