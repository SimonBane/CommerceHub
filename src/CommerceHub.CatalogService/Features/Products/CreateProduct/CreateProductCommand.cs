using CommerceHub.CatalogService.Domain;

namespace CommerceHub.CatalogService.Features.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    decimal BasePrice,
    string? ImageUrl);
