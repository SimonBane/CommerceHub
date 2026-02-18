using CommerceHub.Contracts.Basket;
using CommerceHub.BasketService.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using ZiggyCreatures.Caching.Fusion;

namespace CommerceHub.BasketService.Features.Basket.InitiateCheckout;

public static class InitiateCheckoutHandler
{
    private const string IdempotencyPrefix = "checkout-idempotency:";
    private static readonly TimeSpan IdempotencyDuration = TimeSpan.FromHours(24);

    [Tags("Basket")]
    [WolverinePost("/basket/checkout")]
    public static async Task<IResult> Handle(
        InitiateCheckoutCommand command,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        IBasketStore store,
        IMessageBus bus,
        IFusionCache cache,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var cacheKey = IdempotencyPrefix + idempotencyKey;
            var cached = await cache.TryGetAsync<InitiateCheckoutResponse>(cacheKey, token: ct);
            if (cached.HasValue)
                return Results.Accepted("/basket/checkout", cached.Value);
        }

        var basket = await store.GetAsync(command.CustomerId, ct);
        if (basket == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: "Basket not found.");

        if (basket.Items.Count == 0)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Empty basket",
                detail: "Cannot initiate checkout with an empty basket.");

        var items = basket.Items
            .Select(i => new BasketItemDto(i.ProductId, i.Sku, i.Name, i.UnitPrice, i.Quantity))
            .ToList();

        var @event = new CheckoutInitiatedV1(
            basket.Id,
            basket.CustomerId,
            items,
            basket.Total,
            DateTime.UtcNow);

        await bus.PublishAsync(@event);

        var response = new InitiateCheckoutResponse(basket.Id, items.Count);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var cacheKey = IdempotencyPrefix + idempotencyKey;
            await cache.SetAsync(cacheKey, response, IdempotencyDuration, ct);
        }

        return Results.Accepted("/basket/checkout", response);
    }
}