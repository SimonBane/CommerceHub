using CommerceHub.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.InventoryService.Features.GetStock;

public static class GetStockHandler
{
    [WolverineGet("/inventory/{sku}")]
    public static async Task<IResult> Handle(string sku, InventoryDbContext db, CancellationToken ct)
    {
        var stock = await db.SkuStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Sku == sku, ct);

        if (stock == null)
            return Results.NotFound();

        return Results.Ok(new StockResponse(
            stock.ProductId,
            stock.Sku,
            stock.QuantityOnHand,
            stock.ReservedQuantity,
            stock.AvailableQuantity));
    }

    [WolverineGet("/inventory")]
    public static async Task<IResult> HandleAll(InventoryDbContext db, CancellationToken ct)
    {
        var stocks = await db.SkuStocks
            .AsNoTracking()
            .Select(s => new StockResponse(s.ProductId, s.Sku, s.QuantityOnHand, s.ReservedQuantity, s.AvailableQuantity))
            .ToListAsync(ct);

        return Results.Ok(stocks);
    }
}

public sealed record StockResponse(
    Guid ProductId,
    string Sku,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);
