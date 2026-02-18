namespace CommerceHub.OrderingService.Features.Orders.ListOrders;

public sealed record ListOrdersResponse(
    IReadOnlyList<OrderSummaryDto> Orders,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record OrderSummaryDto(
    Guid OrderId,
    Guid CustomerId,
    decimal Total,
    string CurrencyCode,
    string Status,
    DateTime PlacedAt,
    DateTime? PaidAt,
    DateTime? ShippedAt);
