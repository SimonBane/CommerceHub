using CommerceHub.OrderingService.Domain;
using CommerceHub.OrderingService.Domain.Enums;
using CommerceHub.OrderingService.Domain.Events;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace CommerceHub.OrderingService.Features.Orders.PayOrder;

public static class PayOrderHandler
{
    [WolverinePost("/orders/{orderId:guid}/pay")]
    public static (IResult, IEnumerable<object>) Handle(
        PayOrderCommand command,
        [WriteAggregate("orderId", Required = true, OnMissing = OnMissing.ProblemDetailsWith404)] Order? order)
    {
        if (order == null)
            return (Results.Problem(statusCode: 404, title: "Not Found", detail: "Order not found"), []);

        if (order.Status == OrderStatus.Cancelled)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Cannot pay a cancelled order."), []);

        if (order.Status == OrderStatus.Paid)
            return (Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid state",
                detail: "Order is already paid."), []);

        return (Results.NoContent(), [new OrderPaid(command.PaymentId, command.Amount, command.CurrencyCode)]);
    }
}
