namespace CommerceHub.Contracts.Catalog;

public sealed record CategoryUpdated(
    Guid CategoryId,
    string Name,
    string? Description,
    string Slug,
    Guid? ParentCategoryId,
    DateTime Version);
