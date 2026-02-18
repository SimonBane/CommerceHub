namespace CommerceHub.Contracts.Catalog;

public sealed record CategoryCreated(
    Guid CategoryId,
    string Name,
    string? Description,
    string Slug,
    Guid? ParentCategoryId,
    DateTime CreatedAt);
