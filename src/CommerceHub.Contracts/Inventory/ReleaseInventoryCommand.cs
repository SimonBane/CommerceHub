namespace CommerceHub.Contracts.Inventory;

/// <summary>
/// Command sent by CheckoutOrchestrator to release a reservation (e.g. on payment failure or order cancel).
/// </summary>
public sealed record ReleaseInventoryCommand(
    Guid ReservationId,
    Guid OrderId);
