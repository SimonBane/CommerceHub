namespace CommerceHub.OrderingService.Domain.Events;

/// <summary>
/// Domain events stored in Marten event store (not integration events).
/// </summary>
public sealed record OrderPlaced(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLine> Lines,
    decimal Total,
    string CurrencyCode);