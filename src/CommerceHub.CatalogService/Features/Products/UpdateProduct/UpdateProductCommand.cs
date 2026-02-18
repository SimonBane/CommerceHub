using CommerceHub.CatalogService.Domain;

namespace CommerceHub.CatalogService.Features.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    decimal BasePrice,
    string? ImageUrl);
