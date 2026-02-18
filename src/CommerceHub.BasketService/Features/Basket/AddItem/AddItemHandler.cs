using CommerceHub.BasketService.Infrastructure.Abstractions;
using Wolverine.Http;

namespace CommerceHub.BasketService.Features.Basket.AddItem;

public static class AddItemHandler
{
    [Tags("Basket")]
    [WolverinePost("/basket/items")]
    public static async Task<IResult> Handle(
        AddItemCommand command,
        IBasketStore store,
        CancellationToken ct)
    {
        var basket = await store.GetAsync(command.CustomerId, ct) ?? Domain.Basket.Create(command.CustomerId);

        basket.AddOrUpdateItem(
            command.ProductId,
            command.Sku,
            command.Name,
            command.UnitPrice,
            command.Quantity);

        await store.SaveAsync(basket, ct);

        return Results.Ok(new AddItemResponse(basket.Id, basket.Items.Count, basket.Total));
    }
}