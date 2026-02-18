namespace CommerceHub.BasketService.Features.Basket.AddItem;

public sealed record AddItemResponse(Guid BasketId, int ItemCount, decimal Total);