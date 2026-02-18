using CommerceHub.CatalogService.Domain;
using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Products.CreateProduct;

public static class CreateProductHandler
{
    [Tags("Products")]
    [WolverinePost("/catalog/products")]
    public static async Task<IResult> Handle(
        CreateProductCommand command,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;

        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == command.CategoryId, ct);
        if (!categoryExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid category",
                detail: $"Category with ID {command.CategoryId} does not exist.");
        }

        var skuExists = await dbContext.Products.AnyAsync(p => p.Sku == command.Sku, ct);
        if (skuExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Duplicate SKU",
                detail: $"Product with SKU '{command.Sku}' already exists.");
        }

        var product = Product.CreateNew(command.Name, command.Description, command.Sku, command.CategoryId, command.BasePrice, command.ImageUrl);

        dbContext.Products.Add(product);

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstAsync(c => c.Id == command.CategoryId, ct);

        var version = DateTime.UtcNow;
        var @event = new ProductCreated(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.CategoryId,
            category.Name,
            product.BasePrice,
            product.ImageUrl,
            product.CreatedAt,
            version);

        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        var response = new CreateProductResponse(product.Id, product.Name, product.Sku);
        return Results.Created($"/products/{product.Id}", response);
    }
}
