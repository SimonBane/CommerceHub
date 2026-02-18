using CommerceHub.OrderingService.Domain;
using CommerceHub.OrderingService.Domain.Enums;
using CommerceHub.OrderingService.Domain.Events;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace CommerceHub.OrderingService.Features.Orders.ShipOrder;

public static class ShipOrderHandler
{
    [WolverinePost("/orders/{orderId:guid}/ship")]
    public static (IResult, IEnumerable<object>) Handle(
        ShipOrderCommand command,
        [WriteAggregate("orderId", Required = true, OnMissing = OnMissing.ProblemDetailsWith404)] Order? order)
    {
        if (order == null)
            return (Results.Problem(statusCode: 404, title: "Not Found", detail: "Order not found"), []);

        if (order.Status == OrderStatus.Cancelled)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Cannot ship a cancelled order."), []);

        if (order.Status != OrderStatus.Paid)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Order must be paid before shipping."), []);

        if (order.Status == OrderStatus.Shipped)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Order is already shipped."), []);

        return (Results.NoContent(), [new OrderShipped(command.TrackingNumber)]);
    }
}
