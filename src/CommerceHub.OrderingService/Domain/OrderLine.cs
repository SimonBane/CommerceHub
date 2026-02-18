namespace CommerceHub.OrderingService.Domain;

public sealed record OrderLine(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
