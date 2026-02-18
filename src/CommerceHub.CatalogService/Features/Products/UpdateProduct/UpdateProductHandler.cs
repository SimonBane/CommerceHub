using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Products.UpdateProduct;

public static class UpdateProductHandler
{
    [Tags("Products")]
    [WolverinePut("/products/{id:guid}")]
    public static async Task<IResult> Handle(
        Guid id,
        UpdateProductCommand command,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;

        var product = await dbContext.Products.FindAsync([id], ct);
        if (product == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: $"Product {id} not found.");

        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == command.CategoryId, ct);
        if (!categoryExists)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid category", detail: $"Category {command.CategoryId} does not exist.");

        var skuConflict = await dbContext.Products.AnyAsync(p => p.Sku == command.Sku && p.Id != id, ct);
        if (skuConflict)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Duplicate SKU", detail: $"Another product with SKU '{command.Sku}' exists.");

        var category = await dbContext.Categories.AsNoTracking().FirstAsync(c => c.Id == command.CategoryId, ct);
        
        product.UpdateProduct(
            command.Name,
            command.Description,
            command.Sku,
            command.CategoryId,
            command.BasePrice,
            command.ImageUrl);

        var version = DateTime.UtcNow;
        var @event = new ProductUpdated(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.CategoryId,
            category.Name,
            product.BasePrice,
            product.ImageUrl,
            version);

        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.NoContent();
    }
}
