namespace CommerceHub.BackofficeApi.Features.Catalog.ListProducts;

public sealed record ListProductsResponse(
    IReadOnlyList<ProductListItemDto> Products,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string Sku,
    string CategoryName,
    decimal Price,
    DateTime CreatedAt);
