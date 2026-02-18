using CommerceHub.Contracts.Inventory;
using CommerceHub.InventoryService.Domain;
using CommerceHub.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace CommerceHub.InventoryService.Features.ReserveStock;

/// <summary>
/// Handles ReserveInventoryCommand from CheckoutOrchestrator. Reserves stock and publishes result event.
/// </summary>
public static class ReserveStockHandler
{
    public static async Task Handle(
        ReserveInventoryCommand command,
        IDbContextOutbox<InventoryDbContext> outbox,
        CancellationToken ct)
    {
        var db = outbox.DbContext;

        // Check if we already have a reservation for this order (idempotency)
        var existingReservation = await db.Reservations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.OrderId == command.OrderId && r.Status != ReservationStatus.Released, ct);

        if (existingReservation != null)
        {
            var items = existingReservation.Items
                .Select(i => new ReservedItemDto(i.ProductId, i.Sku, i.Quantity))
                .ToList();
            await outbox.PublishAsync(new InventoryReservedV1(
                existingReservation.Id,
                command.OrderId,
                command.CustomerId,
                items,
                existingReservation.CreatedAt));
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
            return;
        }

        // Validate all SKUs exist and have sufficient stock
        var itemsToReserve = command.Items.ToList();
        var skus = itemsToReserve.Select(i => i.Sku).Distinct().ToList();

        var stocks = await db.SkuStocks
            .Where(s => skus.Contains(s.Sku))
            .ToDictionaryAsync(s => s.Sku, ct);

        foreach (var item in itemsToReserve)
        {
            if (!stocks.TryGetValue(item.Sku, out var stock))
            {
                await outbox.PublishAsync(new InventoryReservationFailedV1(
                    command.OrderId,
                    $"Product {item.Sku} has no stock record.",
                    DateTime.UtcNow));
                await outbox.SaveChangesAndFlushMessagesAsync(ct);
                return;
            }

            if (stock.AvailableQuantity < item.Quantity)
            {
                await outbox.PublishAsync(new InventoryReservationFailedV1(
                    command.OrderId,
                    $"Insufficient stock for {item.Sku}. Available: {stock.AvailableQuantity}, Requested: {item.Quantity}",
                    DateTime.UtcNow));
                await outbox.SaveChangesAndFlushMessagesAsync(ct);
                return;
            }
        }

        // Create reservation and reserve stock
        var reservationItems = itemsToReserve
            .Select(i => (stocks[i.Sku].ProductId, i.Sku, i.Quantity))
            .ToList();

        var reservation = new Reservation(command.OrderId, command.CustomerId, reservationItems);

        foreach (var item in itemsToReserve)
        {
            stocks[item.Sku].TryReserve(item.Quantity);
        }

        db.Reservations.Add(reservation);
        await outbox.PublishAsync(new InventoryReservedV1(
            reservation.Id,
            command.OrderId,
            command.CustomerId,
            itemsToReserve.Select(i => new ReservedItemDto(stocks[i.Sku].ProductId, i.Sku, i.Quantity)).ToList(),
            reservation.CreatedAt));

        await outbox.SaveChangesAndFlushMessagesAsync(ct);
    }
}
