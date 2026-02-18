using CommerceHub.SearchService.Infrastructure.ReadModels;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Wolverine.Http;

namespace CommerceHub.SearchService.Features.Search.SearchProducts;

public static class SearchProductsHandler
{
    [Tags("Products")]
    [WolverineGet("/search/products")]
    public static async Task<SearchProductsResponse> Handle(
        [FromQuery]SearchProductsQuery query,
        IMongoDatabase mongoDatabase,
        CancellationToken ct)
    {
        var collection = mongoDatabase.GetCollection<ProductSearchDocument>("product_search");

        var filterBuilder = Builders<ProductSearchDocument>.Filter;
        var filters = new List<FilterDefinition<ProductSearchDocument>>();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(query.Query, "i")),
                filterBuilder.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(query.Query, "i")),
                filterBuilder.Eq(p => p.Sku, query.Query),
                filterBuilder.AnyIn(p => p.SearchTerms, [query.Query.ToLowerInvariant()]));
            filters.Add(searchFilter);
        }

        if (query.CategoryId.HasValue)
            filters.Add(filterBuilder.Eq(p => p.CategoryId, query.CategoryId.Value));

        if (query.MinPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, query.MaxPrice.Value));

        var filter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: ct);

        var sort = BuildSort(query.SortBy, query.SortDirection);
        var documents = await collection
            .Find(filter)
            .Sort(sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        var results = documents.Select(d => new ProductSearchResult(
            d.Id,
            d.Name,
            d.Description,
            d.Sku,
            d.CategoryName,
            d.Price,
            d.ImageUrl)).ToList();

        return new SearchProductsResponse(results, (int)totalCount, query.Page, query.PageSize);
    }

    private static SortDefinition<ProductSearchDocument> BuildSort(string? sortBy, string? direction)
    {
        var isDesc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var builder = Builders<ProductSearchDocument>.Sort;
        return sortBy?.ToLowerInvariant() switch
        {
            "price" => isDesc ? builder.Descending(p => p.Price) : builder.Ascending(p => p.Price),
            "name" => isDesc ? builder.Descending(p => p.Name) : builder.Ascending(p => p.Name),
            "createdat" or "newest" or "date" => isDesc ? builder.Descending(p => p.CreatedAt) : builder.Ascending(p => p.CreatedAt),
            _ => builder.Descending(p => p.CreatedAt)
        };
    }
}
