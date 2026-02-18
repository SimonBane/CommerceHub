using CommerceHub.Contracts.Inventory;
using CommerceHub.InventoryService.Domain;
using CommerceHub.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace CommerceHub.InventoryService.Features.ReleaseStock;

/// <summary>
/// Handles ReleaseInventoryCommand from CheckoutOrchestrator. Releases reserved stock.
/// </summary>
public static class ReleaseStockHandler
{
    public static async Task Handle(
        ReleaseInventoryCommand command,
        IDbContextOutbox<InventoryDbContext> outbox,
        CancellationToken ct)
    {
        var db = outbox.DbContext;

        var reservation = await db.Reservations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == command.ReservationId && r.OrderId == command.OrderId, ct);

        if (reservation == null || reservation.Status == ReservationStatus.Released)
            return;

        foreach (var item in reservation.Items)
        {
            var stock = await db.SkuStocks.FirstOrDefaultAsync(s => s.Sku == item.Sku, ct);
            stock?.Release(item.Quantity);
        }

        reservation.Release();
        await outbox.SaveChangesAndFlushMessagesAsync(ct);
    }
}
