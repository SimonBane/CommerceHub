using CommerceHub.Contracts.Catalog;
using CommerceHub.SearchService.Infrastructure.ReadModels;
using MongoDB.Driver;

namespace CommerceHub.SearchService.Features.Projections;

public static class ProductDeletedHandler
{
    public static async Task Handle(
        ProductDeleted evt,
        IMongoDatabase database,
        CancellationToken ct)
    {
        var collection = database.GetCollection<ProductSearchDocument>("product_search");
        
        var builder = Builders<ProductSearchDocument>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.Id, evt.ProductId),
            builder.Lt(x => x.Version, evt.Version));

        await collection.DeleteOneAsync(filter, ct);
    }
}
