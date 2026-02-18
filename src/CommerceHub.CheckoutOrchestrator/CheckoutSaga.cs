using CommerceHub.Contracts.Basket;
using CommerceHub.Contracts.Inventory;
using CommerceHub.Contracts.Order;
using CommerceHub.Contracts.Payment;
using Wolverine;

namespace CommerceHub.CheckoutOrchestrator;

/// <summary>
/// Saga coordinating checkout: Reserve inventory -> Initiate payment -> Create order.
/// Compensates by releasing inventory on payment failure.
/// </summary>
public partial class CheckoutSaga : Saga
{
    public string? Id { get; set; } = null!; // OrderId as string for saga identity

    public Guid OrderId => Guid.Parse(Id!);
    public Guid BasketId { get; set; }
    public Guid CustomerId { get; set; }
    public List<CheckoutSagaItem> Items { get; set; } = [];
    public decimal Total { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    public Guid? ReservationId { get; set; }
    public CheckoutSagaStep Step { get; set; }
}

public class CheckoutSagaItem
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public enum CheckoutSagaStep
{
    AwaitingReservation,
    Reserved,
    PaymentInitiated,
    OrderCreated,
    Failed
}
