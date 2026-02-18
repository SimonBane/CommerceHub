using CommerceHub.BackofficeApi.Constants;
using CommerceHub.BackofficeApi.Infrastructure.ReadModels;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Catalog.ListProducts;

/// <summary>
/// Lists products from MongoDB product_search projection (admin view).
/// </summary>
public static class ListProductsHandler
{
    [WolverineGet("/backoffice/products")]
    public static async Task<ListProductsResponse> Handle(
        [FromQuery] ListProductsQuery query,
        IMongoDatabase mongoDatabase,
        CancellationToken ct)
    {
        var collection = mongoDatabase.GetCollection<ProductReadModel>(CollectionNames.ProductSearch);

        var filterBuilder = Builders<ProductReadModel>.Filter;
        var filters = new List<FilterDefinition<ProductReadModel>>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(query.Search, "i")),
                filterBuilder.Regex(p => p.Sku, new MongoDB.Bson.BsonRegularExpression(query.Search, "i")));
            filters.Add(searchFilter);
        }

        if (query.CategoryId.HasValue)
            filters.Add(filterBuilder.Eq(p => p.CategoryId, query.CategoryId.Value));

        var filter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

        var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: ct);

        var sort = Builders<ProductReadModel>.Sort.Descending(p => p.CreatedAt);
        var documents = await collection
            .Find(filter)
            .Sort(sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => new ProductListItemDto(
            d.Id,
            d.Name,
            d.Sku,
            d.CategoryName,
            d.Price,
            d.CreatedAt)).ToList();

        return new ListProductsResponse(items, (int)totalCount, query.Page, query.PageSize);
    }
}
