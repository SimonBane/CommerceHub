using CommerceHub.Contracts.Catalog;
using CommerceHub.SearchService.Infrastructure.ReadModels;
using MongoDB.Driver;

namespace CommerceHub.SearchService.Features.Projections;

public static class CategoryUpdatedHandler
{
    public static async Task Handle(
        CategoryUpdated evt,
        IMongoDatabase database,
        CancellationToken ct)
    {
        var collection = database.GetCollection<ProductSearchDocument>("product_search");
        var filter = Builders<ProductSearchDocument>.Filter.And(
            Builders<ProductSearchDocument>.Filter.Eq(x => x.CategoryId, evt.CategoryId),
            Builders<ProductSearchDocument>.Filter.Lt(x => x.Version, evt.Version));
        var update = Builders<ProductSearchDocument>.Update
            .Set(x => x.CategoryName, evt.Name)
            .Set(x => x.Version, evt.Version);
        await collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}
