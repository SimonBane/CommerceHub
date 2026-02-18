using CommerceHub.Contracts.Catalog;
using CommerceHub.SearchService.Infrastructure.ReadModels;
using MongoDB.Driver;
using static CommerceHub.SearchService.Infrastructure.Helpers.ProductProjectionHelper;

namespace CommerceHub.SearchService.Features.Projections;

public static class ProductCreatedHandler
{
    public static async Task Handle(
        ProductCreated evt,
        IMongoDatabase database,
        CancellationToken ct)
    {
        var collection = database.GetCollection<ProductSearchDocument>("product_search");

        var builder = Builders<ProductSearchDocument>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.Id, evt.ProductId),
            builder.Or(
                builder.Exists(x => x.Version, false),
                builder.Lt(x => x.Version, evt.Version)));

        var terms = BuildSearchTerms(evt.Name, evt.Description, evt.Sku);
        var doc = new ProductSearchDocument
        {
            Id = evt.ProductId,
            Name = evt.Name,
            Description = evt.Description,
            Sku = evt.Sku,
            CategoryName = evt.CategoryName,
            CategoryId = evt.CategoryId,
            Price = evt.Price,
            ImageUrl = evt.ImageUrl,
            CreatedAt = evt.CreatedAt,
            Version = evt.Version,
            SearchTerms = terms
        };

        await collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }
}
