namespace CommerceHub.BackofficeApi.Features.Catalog.ListProducts;

public sealed record ListProductsQuery(
    string? Search = null,
    Guid? CategoryId = null,
    int Page = 1,
    int PageSize = 20);
