using CommerceHub.BasketService.Infrastructure.Abstractions;
using Wolverine.Http;

namespace CommerceHub.BasketService.Features.Basket.GetBasket;

public static class GetBasketHandler
{
    [Tags("Basket")]
    [WolverineGet("/basket/{customerId:guid}")]
    public static async Task<IResult> Handle(
        Guid customerId,
        IBasketStore store,
        CancellationToken ct)
    {
        var basket = await store.GetAsync(customerId, ct);
        if (basket == null)
            return Results.NotFound();

        var response = new GetBasketResponse(
            basket.Id,
            basket.CustomerId,
            basket.Items.Select(i => new BasketItemResponse(i.ProductId, i.Sku, i.Name, i.UnitPrice, i.Quantity)).ToList(),
            basket.Total,
            basket.UpdatedAt);

        return Results.Ok(response);
    }
}