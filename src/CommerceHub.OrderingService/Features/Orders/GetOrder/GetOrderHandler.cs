using CommerceHub.OrderingService.Domain;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace CommerceHub.OrderingService.Features.Orders.GetOrder;

public static class GetOrderHandler
{
    [WolverineGet("/orders/{orderId:guid}")]
    public static GetOrderResponse Handle(
        [ReadAggregate(Required = true, OnMissing = OnMissing.ProblemDetailsWith404, MissingMessage = "Order not found")] Order order)
    {
        return new GetOrderResponse(
            order.Id,
            order.CustomerId,
            order.Lines.Select(l => new OrderLineResponse(l.ProductId, l.Sku, l.Name, l.UnitPrice, l.Quantity)).ToList(),
            order.Total,
            order.CurrencyCode,
            order.Status.ToString(),
            order.PlacedAt,
            order.PaidAt,
            order.ShippedAt,
            order.CancelledAt,
            order.TrackingNumber);
    }
}
