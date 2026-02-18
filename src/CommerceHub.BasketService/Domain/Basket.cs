namespace CommerceHub.BasketService.Domain;

public sealed class Basket
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public List<BasketItem> Items { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

    private Basket(Guid customerId, List<BasketItem> items)
    {
        Id = Guid.CreateVersion7();
        CustomerId = customerId;
        Items = items;
        CreatedAt = DateTime.UtcNow;
    }

    public static Basket Create(Guid customerId)
    {
        return new Basket(customerId, []);
    }

    public void AddOrUpdateItem(Guid productId, string sku, string name, decimal unitPrice, int quantity)
    {
        var existing = Items.Find(x => x.ProductId == productId);
        if (existing != null)
        {
            var newQty = existing.Quantity + quantity;
            if (newQty <= 0)
            {
                Items.Remove(existing);
            }
            else
            {
                Items[Items.IndexOf(existing)] = existing with { Quantity = newQty };
            }
        }
        else if (quantity > 0)
        {
            Items.Add(new BasketItem(productId, sku, name, unitPrice, quantity));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetItemQuantity(Guid productId, int quantity)
    {
        var existing = Items.Find(x => x.ProductId == productId);
        if (existing == null)
            return;

        if (quantity <= 0)
        {
            Items.Remove(existing);
        }
        else
        {
            Items[Items.IndexOf(existing)] = existing with { Quantity = quantity };
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        Items.RemoveAll(x => x.ProductId == productId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal Total => Items.Sum(x => x.LineTotal);
}
