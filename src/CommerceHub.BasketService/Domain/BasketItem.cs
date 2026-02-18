namespace CommerceHub.BasketService.Domain;

public sealed record BasketItem(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
