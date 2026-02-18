namespace CommerceHub.Contracts.Catalog;

public sealed record ProductUpdated(
    Guid ProductId,
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    string CategoryName,
    decimal Price,
    string? ImageUrl,
    DateTime Version);
