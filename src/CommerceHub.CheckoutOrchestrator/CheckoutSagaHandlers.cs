using CommerceHub.Contracts.Inventory;
using CommerceHub.Contracts.Order;
using CommerceHub.Contracts.Payment;
using Wolverine;
using Wolverine.Persistence.Sagas;

namespace CommerceHub.CheckoutOrchestrator;

/// <summary>
/// Handlers for CheckoutSaga - process events from Inventory, Payment, and Ordering services.
/// Uses [SagaIdentityFrom] to correlate by OrderId since our saga Id is OrderId as string.
/// </summary>
public partial class CheckoutSaga
{
    public InitiatePaymentCommand Handle(
        [SagaIdentityFrom(nameof(InventoryReservedV1.OrderId))] InventoryReservedV1 msg)
    {
        ReservationId = msg.ReservationId;
        Step = CheckoutSagaStep.PaymentInitiated;

        return new InitiatePaymentCommand(
            OrderId,
            CustomerId,
            Total,
            CurrencyCode,
            IdempotencyKey: $"checkout-{OrderId}");
    }

    public void Handle(
        [SagaIdentityFrom(nameof(InventoryReservationFailedV1.OrderId))] InventoryReservationFailedV1 msg)
    {
        Step = CheckoutSagaStep.Failed;
        MarkCompleted();
    }

    public CreateOrderCommand Handle(
        [SagaIdentityFrom(nameof(PaymentAuthorizedV1.OrderId))] PaymentAuthorizedV1 msg)
    {
        Step = CheckoutSagaStep.OrderCreated;

        var lines = Items
            .Select(i => new OrderLineDto(i.ProductId, i.Sku, i.Name, i.UnitPrice, i.Quantity))
            .ToList();

        return new CreateOrderCommand(
            OrderId,
            CustomerId,
            lines,
            Total,
            CurrencyCode);
    }

    public ReleaseInventoryCommand? Handle(
        [SagaIdentityFrom(nameof(PaymentFailedV1.OrderId))] PaymentFailedV1 msg)
    {
        Step = CheckoutSagaStep.Failed;

        if (ReservationId.HasValue)
        {
            MarkCompleted();
            return new ReleaseInventoryCommand(ReservationId.Value, OrderId);
        }

        MarkCompleted();
        return null;
    }

    public void Handle(
        [SagaIdentityFrom(nameof(OrderPlacedV1.OrderId))] OrderPlacedV1 msg)
    {
        Step = CheckoutSagaStep.OrderCreated;
        MarkCompleted();
    }

    public static void NotFound(InventoryReservedV1 msg) { }
    public static void NotFound(InventoryReservationFailedV1 msg) { }
    public static void NotFound(PaymentAuthorizedV1 msg) { }
    public static void NotFound(PaymentFailedV1 msg) { }
    public static void NotFound(OrderPlacedV1 msg) { }
}
