namespace CommerceHub.InventoryService.Domain;

/// <summary>
/// A reservation of stock for an order. Created when ReserveInventory succeeds.
/// </summary>
public sealed class Reservation
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }

    public ICollection<ReservationItem> Items { get; init; } = [];

    private Reservation() { }

    public Reservation(Guid orderId, Guid customerId, IReadOnlyList<(Guid ProductId, string Sku, int Quantity)> items, TimeSpan? expiry = null)
    {
        Id = Guid.CreateVersion7();
        OrderId = orderId;
        CustomerId = customerId;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(15));
        Items = items.Select(i => new ReservationItem(Id, i.ProductId, i.Sku, i.Quantity)).ToList();
    }

    public void Release()
    {
        Status = ReservationStatus.Released;
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
    }
}
