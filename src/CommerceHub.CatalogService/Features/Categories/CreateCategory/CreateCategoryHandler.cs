using CommerceHub.CatalogService.Domain;
using CommerceHub.CatalogService.Infrastructure.Persistence;
using CommerceHub.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.CatalogService.Features.Categories.CreateCategory;

public class CreateCategoryHandler
{
    [Tags("Categories")]
    [WolverinePost("/catalog/categories")]
    public static async Task<IResult> Handle(
        CreateCategoryCommand command,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var dbContext = outbox.DbContext;
        
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

        var slugExists = await dbContext.Categories.AnyAsync(c => c.Slug == command.Slug, ct);
        if (slugExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Duplicate slug",
                detail: $"Category with slug '{command.Slug}' already exists.");
        }
        
        var category = Category.CreateNew(command.Name, command.Slug, command.Description, command.ParentCategoryId);
        await dbContext.Categories.AddAsync(category, ct);
        
        var @event = new CategoryCreated(
            category.Id,
            category.Name,
            category.Description,
            category.Slug,
            category.ParentCategoryId,
            category.CreatedAt);

        await outbox.PublishAsync(@event);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        var response = new CategoryCreationResponse(category.Id, category.Name, category.Slug);
        return Results.Created($"/catalog/categories/{category.Id}", response);
    }
}
