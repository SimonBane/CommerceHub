using CommerceHub.Contracts.Order;
using CommerceHub.OrderingService.Domain;
using CommerceHub.OrderingService.Domain.Events;
using JasperFx.Events;
using Marten;
using Marten.Events;

namespace CommerceHub.OrderingService.Features.Orders.EventForwarding;

/// <summary>
/// When Marten events are committed, they are forwarded to Wolverine. These handlers
/// publish integration events (Contracts) to RabbitMQ for other services.
/// </summary>
public static class OrderEventForwardingHandlers
{
    public static OrderPlacedV1 Handle(IEvent<OrderPlaced> evt)
    {
        var order = evt.Data;
        return new OrderPlacedV1(
            evt.StreamId,
            order.CustomerId,
            order.Lines.Select(l => new OrderLineDto(l.ProductId, l.Sku, l.Name, l.UnitPrice, l.Quantity)).ToList(),
            order.Total,
            order.CurrencyCode,
            evt.Timestamp.UtcDateTime);
    }

    public static async Task<OrderPaidV1> Handle(IEvent<OrderPaid> evt, IQuerySession session)
    {
        var paid = evt.Data;
        var order = await session.LoadAsync<Order>(evt.StreamId);
        return new OrderPaidV1(
            evt.StreamId,
            order!.CustomerId,
            paid.PaymentId,
            paid.Amount,
            paid.CurrencyCode,
            evt.Timestamp.UtcDateTime);
    }

    public static async Task<OrderCancelledV1> Handle(IEvent<OrderCancelled> evt, IQuerySession session)
    {
        var cancelled = evt.Data;
        var order = await session.LoadAsync<Order>(evt.StreamId);
        return new OrderCancelledV1(
            evt.StreamId,
            order!.CustomerId,
            cancelled.Reason,
            evt.Timestamp.UtcDateTime);
    }

    public static async Task<OrderShippedV1> Handle(IEvent<OrderShipped> evt, IQuerySession session)
    {
        var shipped = evt.Data;
        var order = await session.LoadAsync<Order>(evt.StreamId);
        return new OrderShippedV1(
            evt.StreamId,
            order!.CustomerId,
            shipped.TrackingNumber,
            evt.Timestamp.UtcDateTime);
    }
}
