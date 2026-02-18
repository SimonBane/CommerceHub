using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Categories.DeleteCategory;

public class DeleteCategoryHandler
{
    [Tags("Categories")]
    [WolverineDelete("/catalog/categories/{id:guid}"), EmptyResponse]
    public static async Task<IResult> Handle(
        Guid id,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;
        
        var category = await  dbContext.Categories.FindAsync([id], ct);
        if (category == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: $"Category {id} not found.");
        
        var hasProducts = await dbContext.Products.AnyAsync(p => p.CategoryId == category.Id, ct);
        if (hasProducts)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Category in use", detail: "Cannot delete category that has products.");

        dbContext.Categories.Remove(category);

        var @event = new CategoryDeleted(category.Id, DateTime.UtcNow);
        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.NoContent();
    }
}
