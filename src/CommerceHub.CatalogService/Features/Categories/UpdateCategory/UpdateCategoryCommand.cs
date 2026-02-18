namespace CommerceHub.CatalogService.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    string Name,
    string? Description,
    string Slug,
    Guid? ParentCategoryId);
