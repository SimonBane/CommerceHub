namespace CommerceHub.BasketService.Features.Basket.AddItem;

public sealed record AddItemCommand(
    Guid CustomerId,
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity = 1);
