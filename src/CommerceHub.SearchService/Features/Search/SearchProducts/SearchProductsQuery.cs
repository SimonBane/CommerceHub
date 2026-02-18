namespace CommerceHub.SearchService.Features.Search.SearchProducts;

public sealed record SearchProductsQuery(
    string? Query = null,
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = "newest",
    string? SortDirection = "desc");
