using CommerceHub.BasketService.Infrastructure.Abstractions;
using Wolverine.Http;

namespace CommerceHub.BasketService.Features.Basket.RemoveItem;

public static class RemoveItemHandler
{
    [Tags("Basket")]
    [WolverineDelete("/basket/{customerId:guid}/items/{productId:guid}")]
    public static async Task<IResult> Handle(
        Guid customerId,
        Guid productId,
        IBasketStore store,
        CancellationToken ct)
    {
        var basket = await store.GetAsync(customerId, ct);
        if (basket == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: "Basket not found.");

        basket.RemoveItem(productId);
        await store.SaveAsync(basket, ct);
        return Results.NoContent();
    }
}
