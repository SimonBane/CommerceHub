namespace CommerceHub.BackofficeApi.Features.Catalog.CreateProduct;

public sealed record CreateProductRequest(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    decimal BasePrice,
    string? ImageUrl);
