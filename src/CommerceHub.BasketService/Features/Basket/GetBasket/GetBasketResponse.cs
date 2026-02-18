namespace CommerceHub.BasketService.Features.Basket.GetBasket;

public sealed record GetBasketResponse(
    Guid BasketId,
    Guid CustomerId,
    IReadOnlyList<BasketItemResponse> Items,
    decimal Total,
    DateTime UpdatedAt);