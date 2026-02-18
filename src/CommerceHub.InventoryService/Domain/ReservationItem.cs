namespace CommerceHub.InventoryService.Domain;

public sealed class ReservationItem
{
    public Guid Id { get; init; }
    public Guid ReservationId { get; init; }
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }

    public Reservation? Reservation { get; init; }

    private ReservationItem() { }

    public ReservationItem(Guid reservationId, Guid productId, string sku, int quantity)
    {
        Id = Guid.CreateVersion7();
        ReservationId = reservationId;
        ProductId = productId;
        Sku = sku;
        Quantity = quantity;
    }
}
