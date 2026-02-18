using CommerceHub.BasketService.Domain;

namespace CommerceHub.BasketService.Infrastructure.Abstractions;

public interface IBasketStore
{
    Task<Basket?> GetAsync(Guid customerId, CancellationToken ct = default);
    Task SaveAsync(Basket basket, CancellationToken ct = default);
    Task RemoveAsync(Guid customerId, CancellationToken ct = default);
}