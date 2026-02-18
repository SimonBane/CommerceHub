using CommerceHub.OrderingService.Domain.Enums;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;

namespace CommerceHub.OrderingService.Features.Orders.ListOrders;

public static class ListOrdersHandler
{
    [WolverineGet("/orders")]
    public static async Task<ListOrdersResponse> Handle(
        [FromQuery] ListOrdersQuery query,
        IQuerySession session,
        CancellationToken ct)
    {
        IQueryable<Domain.Order> queryable = session.Query<Domain.Order>()
            .OrderByDescending(o => o.PlacedAt);

        if (query.CustomerId.HasValue)
            queryable = queryable.Where(o => o.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<OrderStatus>(query.Status, ignoreCase: true, out var status))
            queryable = queryable.Where(o => o.Status == status);

        var totalCount = await queryable.CountAsync(ct);

        var orders = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.CustomerId,
                o.Total,
                o.CurrencyCode,
                o.Status.ToString(),
                o.PlacedAt,
                o.PaidAt,
                o.ShippedAt))
            .ToListAsync(ct);

        return new ListOrdersResponse(orders, totalCount, query.Page, query.PageSize);
    }
}
