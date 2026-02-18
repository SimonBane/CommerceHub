using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Products.DeleteProduct;

public static class DeleteProductHandler
{
    [Tags("Products")]
    [WolverineDelete("/products/{id:guid}")]
    public static async Task<IResult> Handle(
        Guid id,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;

        var product = await dbContext.Products.FindAsync([id], ct);
        if (product == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: $"Product {id} not found.");

        dbContext.Products.Remove(product);

        var version = DateTime.UtcNow;
        var @event = new ProductDeleted(product.Id, version);

        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.NoContent();
    }
}
