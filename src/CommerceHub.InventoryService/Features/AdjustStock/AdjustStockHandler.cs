using CommerceHub.InventoryService.Domain;
using CommerceHub.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace CommerceHub.InventoryService.Features.AdjustStock;

public static class AdjustStockHandler
{
    [WolverinePost("/inventory/adjust")]
    public static async Task<IResult> Handle(
        AdjustStockCommand command,
        InventoryDbContext db,
        CancellationToken ct)
    {
        var stock = await db.SkuStocks.FirstOrDefaultAsync(s => s.Sku == command.Sku, ct);
        if (stock == null)
        {
            if (command.Delta < 0)
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid",
                    detail: "Cannot reduce stock for non-existent SKU.");

            stock = new SkuStock(command.ProductId, command.Sku, 0);
            db.SkuStocks.Add(stock);
        }

        stock.AdjustQuantity(command.Delta);

        if (stock.QuantityOnHand < stock.ReservedQuantity)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid",
                detail: $"Adjustment would result in quantity ({stock.QuantityOnHand}) below reserved ({stock.ReservedQuantity}).");

        await db.SaveChangesAsync(ct);
        return Results.Ok(new GetStock.StockResponse(stock.ProductId, stock.Sku, stock.QuantityOnHand, stock.ReservedQuantity, stock.AvailableQuantity));
    }
}
