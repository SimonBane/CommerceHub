using CommerceHub.BasketService.Domain;
using CommerceHub.BasketService.Infrastructure.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace CommerceHub.BasketService.Infrastructure;

public sealed class BasketStore(IFusionCache cache) : IBasketStore
{
    private const string KeyPrefix = "basket:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(7);

    public async Task<Basket?> GetAsync(Guid customerId, CancellationToken ct = default)
    {
        var key = KeyPrefix + customerId;
        var maybeValue = await cache.TryGetAsync<Basket>(key, token: ct);
        return maybeValue.HasValue ? maybeValue.Value : null;
    }

    public async Task SaveAsync(Basket basket, CancellationToken ct = default)
    {
        var key = KeyPrefix + basket.CustomerId;
        await cache.SetAsync(key, basket, DefaultExpiration, ct);
    }

    public async Task RemoveAsync(Guid customerId, CancellationToken ct = default)
    {
        var key = KeyPrefix + customerId;
        await cache.RemoveAsync(key, token: ct);
    }
}
