namespace CommerceHub.Contracts.Inventory;

/// <summary>
/// Published when inventory reservation fails (insufficient stock).
/// Consumed by CheckoutOrchestrator to abort checkout and notify user.
/// </summary>
public sealed record InventoryReservationFailedV1(
    Guid OrderId,
    string Reason,
    DateTime FailedAt);
