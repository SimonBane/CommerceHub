using CommerceHub.OrderingService.Domain;
using CommerceHub.OrderingService.Domain.Enums;
using CommerceHub.OrderingService.Domain.Events;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace CommerceHub.OrderingService.Features.Orders.CancelOrder;

public static class CancelOrderHandler
{
    [WolverinePost("/orders/{orderId:guid}/cancel")]
    public static (IResult, IEnumerable<object>) Handle(
        CancelOrderCommand command,
        [WriteAggregate("orderId", Required = true, OnMissing = OnMissing.ProblemDetailsWith404)] Order? order)
    {
        if (order == null)
            return (Results.Problem(statusCode: 404, title: "Not Found", detail: "Order not found"), []);

        if (order.Status == OrderStatus.Cancelled)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Order is already cancelled."), []);

        if (order.Status == OrderStatus.Shipped)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Cannot cancel a shipped order."), []);

        return (Results.NoContent(), [new OrderCancelled(command.Reason)]);
    }
}
