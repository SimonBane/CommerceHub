using CommerceHub.Contracts.Catalog;
using CommerceHub.SearchService.Infrastructure.ReadModels;
using MongoDB.Driver;

namespace CommerceHub.SearchService.Features.Projections;

public static class CategoryDeletedHandler
{
    public static async Task Handle(
        CategoryDeleted evt,
        IMongoDatabase database,
        CancellationToken ct)
    {
        var collection = database.GetCollection<ProductSearchDocument>("product_search");

        var builder = Builders<ProductSearchDocument>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.CategoryId, evt.CategoryId),
            builder.Lt(x => x.Version, evt.Version));

        var update = Builders<ProductSearchDocument>.Update
            .Set(x => x.CategoryName, "[Deleted]")
            .Set(x => x.Version, evt.Version);

        await collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}
