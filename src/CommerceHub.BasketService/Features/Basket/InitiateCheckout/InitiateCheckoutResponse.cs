namespace CommerceHub.BasketService.Features.Basket.InitiateCheckout;

public sealed record InitiateCheckoutResponse(Guid BasketId, int ItemCount);