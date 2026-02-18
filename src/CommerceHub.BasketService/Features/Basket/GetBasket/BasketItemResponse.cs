namespace CommerceHub.BasketService.Features.Basket.GetBasket;

public sealed record BasketItemResponse(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    int Quantity);