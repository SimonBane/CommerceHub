namespace CommerceHub.Contracts.Inventory;

/// <summary>
/// Command sent by CheckoutOrchestrator to reserve stock for an order.
/// </summary>
public sealed record ReserveInventoryCommand(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<ReserveInventoryItemDto> Items);

public sealed record ReserveInventoryItemDto(
    Guid ProductId,
    string Sku,
    int Quantity);
