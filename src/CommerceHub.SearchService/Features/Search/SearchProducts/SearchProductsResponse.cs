namespace CommerceHub.SearchService.Features.Search.SearchProducts;

public sealed record SearchProductsResponse(
    IReadOnlyList<ProductSearchResult> Results,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ProductSearchResult(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    string CategoryName,
    decimal Price,
    string? ImageUrl);
