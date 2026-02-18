namespace CommerceHub.BasketService.Features.Basket.UpdateItem;

public sealed record UpdateItemResponse(Guid BasketId, int ItemCount, decimal Total);