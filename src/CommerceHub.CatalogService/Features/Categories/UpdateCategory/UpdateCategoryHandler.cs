using CommerceHub.CatalogService.Domain;
using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Categories.UpdateCategory;

public static class UpdateCategoryHandler
{
    [Tags("Categories")]
    [WolverinePut("/catalog/categories/{id:guid}")]
    public static async Task<IResult> Handle(
        Guid id,
        UpdateCategoryCommand command,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;
        
        var category = await dbContext.Categories.FindAsync([id], ct);
        if (category == null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: $"Category {id} not found.");
        
        if (command.ParentCategoryId.HasValue && command.ParentCategoryId.Value == id)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid parent",
                detail: "Category cannot be its own parent.");
        }

        if (command.ParentCategoryId.HasValue)
        {
            var parentExists = await dbContext.Categories.AnyAsync(c => c.Id == command.ParentCategoryId.Value, ct);
            if (!parentExists)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid parent category",
                    detail: $"Parent category with ID {command.ParentCategoryId} does not exist.");
            }
        }

        var slugConflict = await dbContext.Categories.AnyAsync(c => c.Slug == command.Slug && c.Id != id, ct);
        if (slugConflict)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Duplicate slug",
                detail: $"Another category with slug '{command.Slug}' exists.");
        }
        
        var @event = new CategoryUpdated(
            category.Id,
            category.Name,
            category.Description,
            category.Slug,
            category.ParentCategoryId,
            DateTime.UtcNow);

        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.NoContent();
    }
}
