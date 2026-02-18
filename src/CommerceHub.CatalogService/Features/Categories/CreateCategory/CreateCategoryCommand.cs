namespace CommerceHub.CatalogService.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description, string Slug, Guid? ParentCategoryId);
