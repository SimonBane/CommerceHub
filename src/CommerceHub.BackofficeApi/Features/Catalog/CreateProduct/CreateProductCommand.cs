namespace CommerceHub.BackofficeApi.Features.Catalog.CreateProduct;

/// <summary>
/// Command matching CatalogService contract for creating products.
/// </summary>
internal sealed record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    decimal BasePrice,
    string? ImageUrl);
