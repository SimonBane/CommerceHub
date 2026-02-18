namespace CommerceHub.InventoryService.Domain;

/// <summary>
/// Stock level for a product SKU. Uses optimistic concurrency via RowVersion.
/// </summary>
public sealed class SkuStock
{
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public int QuantityOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    private SkuStock() { }

    public SkuStock(Guid productId, string sku, int quantityOnHand)
    {
        ProductId = productId;
        Sku = sku;
        QuantityOnHand = quantityOnHand;
        ReservedQuantity = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool TryReserve(int quantity)
    {
        if (AvailableQuantity < quantity)
            return false;
        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void Release(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustQuantity(int delta)
    {
        QuantityOnHand = Math.Max(0, QuantityOnHand + delta);
        UpdatedAt = DateTime.UtcNow;
    }
}
