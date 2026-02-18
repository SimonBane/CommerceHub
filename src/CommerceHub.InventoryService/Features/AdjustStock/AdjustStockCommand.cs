namespace CommerceHub.InventoryService.Features.AdjustStock;

public sealed record AdjustStockCommand(
    Guid ProductId,
    string Sku,
    int Delta);
