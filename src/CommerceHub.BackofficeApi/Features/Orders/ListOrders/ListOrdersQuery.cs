namespace CommerceHub.BackofficeApi.Features.Orders.ListOrders;

public sealed record ListOrdersQuery(
    Guid? CustomerId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20);
