using CommerceHub.Contracts.Order;
using CommerceHub.OrderingService.Domain;
using CommerceHub.OrderingService.Domain.Events;
using Wolverine;
using Wolverine.Marten;

namespace CommerceHub.OrderingService.Features.Orders.CreateOrder;

/// <summary>
/// Handles CreateOrderCommand from CheckoutOrchestrator. Creates a new event-sourced order.
/// </summary>
public static class CreateOrderHandler
{
    public static (IEnumerable<object>, OutgoingMessages) Handle(
        CreateOrderCommand command,
        [WriteAggregate("OrderId", Required = false)] Order? order)
    {
        if (order != null)
            throw new InvalidOperationException($"Order {command.OrderId} already exists.");

        var lines = command.Lines
            .Select(l => new OrderLine(l.ProductId, l.Sku, l.Name, l.UnitPrice, l.Quantity))
            .ToList();

        var domainEvent = new OrderPlaced(
            command.OrderId,
            command.CustomerId,
            lines,
            command.Total,
            command.CurrencyCode);

        var integrationEvent = new OrderPlacedV1(
            command.OrderId,
            command.CustomerId,
            command.Lines,
            command.Total,
            command.CurrencyCode,
            DateTime.UtcNow);

        var messages = new OutgoingMessages { integrationEvent };

        return ([domainEvent], messages);
    }
}
