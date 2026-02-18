using CommerceHub.BasketService.Infrastructure.Abstractions;
using Wolverine.Http;

namespace CommerceHub.BasketService.Features.Basket.ClearBasket;

public static class ClearBasketHandler
{
    [Tags("Basket")]
    [WolverineDelete("/basket/{customerId:guid}")]
    public static async Task<IResult> Handle(
        Guid customerId,
        IBasketStore store,
        CancellationToken ct)
    {
        await store.RemoveAsync(customerId, ct);
        return Results.NoContent();
    }
}
