using CommerceHub.BasketService.Infrastructure.Abstractions;
using Wolverine.Http;

namespace CommerceHub.BasketService.Features.Basket.UpdateItem;

public static class UpdateItemHandler
{
    [Tags("Basket")]
    [WolverinePut("/basket/{customerId:guid}/items/{productId:guid}")]
    public static async Task<IResult> Handle(
        Guid customerId,
        Guid productId,
        UpdateItemCommand command,
        IBasketStore store,
        CancellationToken ct)
    {
        var basket = await store.GetAsync(customerId, ct);
        if (basket == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: "Basket not found.");

        basket.SetItemQuantity(productId, command.Quantity);
        await store.SaveAsync(basket, ct);
        return Results.Ok(new UpdateItemResponse(basket.Id, basket.Items.Count, basket.Total));
    }
}