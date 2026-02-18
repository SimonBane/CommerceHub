using CommerceHub.Contracts.Catalog;
using CommerceHub.SearchService.Infrastructure.ReadModels;
using MongoDB.Driver;
using static CommerceHub.SearchService.Infrastructure.Helpers.ProductProjectionHelper;

namespace CommerceHub.SearchService.Features.Projections;

public static class ProductUpdatedHandler
{
    public static async Task Handle(
        ProductUpdated evt,
        IMongoDatabase database,
        CancellationToken ct)
    {
        var collection = database.GetCollection<ProductSearchDocument>("product_search");

        var builder = Builders<ProductSearchDocument>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.Id, evt.ProductId),
            builder.Lt(x => x.Version, evt.Version));

        var terms = BuildSearchTerms(evt.Name, evt.Description, evt.Sku);
        var update = Builders<ProductSearchDocument>.Update
            .Set(x => x.Name, evt.Name)
            .Set(x => x.Description, evt.Description)
            .Set(x => x.Sku, evt.Sku)
            .Set(x => x.CategoryName, evt.CategoryName)
            .Set(x => x.CategoryId, evt.CategoryId)
            .Set(x => x.Price, evt.Price)
            .Set(x => x.ImageUrl, evt.ImageUrl)
            .Set(x => x.Version, evt.Version)
            .Set(x => x.SearchTerms, terms);

        await collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}
