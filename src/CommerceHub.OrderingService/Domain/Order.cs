using CommerceHub.OrderingService.Domain.Enums;
using CommerceHub.OrderingService.Domain.Events;

namespace CommerceHub.OrderingService.Domain;

/// <summary>
/// Event-sourced Order aggregate. Marten projects state by applying events.
/// </summary>
public sealed class Order
{
    public Guid Id { get; }
    public int Version { get; set; }
    public Guid CustomerId { get; }
    public List<OrderLine> Lines { get; } = [];
    public decimal Total { get; }
    public string CurrencyCode { get; } = "USD";
    public OrderStatus Status { get; private set; }
    public DateTime PlacedAt { get; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? CancelReason { get; private set; }

    public Order() { }

    public Order(OrderPlaced placed)
    {
        Id = placed.OrderId;
        CustomerId = placed.CustomerId;
        Lines = placed.Lines.ToList();
        Total = placed.Total;
        CurrencyCode = placed.CurrencyCode;
        Status = OrderStatus.Placed;
        PlacedAt = DateTime.UtcNow;
    }

    public void Apply(OrderPaid paid)
    {
        PaymentId = paid.PaymentId;
        PaidAt = DateTime.UtcNow;
        Status = OrderStatus.Paid;
    }

    public void Apply(OrderCancelled cancelled)
    {
        CancelReason = cancelled.Reason;
        CancelledAt = DateTime.UtcNow;
        Status = OrderStatus.Cancelled;
    }

    public void Apply(OrderShipped shipped)
    {
        TrackingNumber = shipped.TrackingNumber;
        ShippedAt = DateTime.UtcNow;
        Status = OrderStatus.Shipped;
    }
}