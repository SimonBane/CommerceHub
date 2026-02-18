using CommerceHub.BackofficeApi.Constants;
using MongoDB.Bson;
using MongoDB.Driver;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Reporting.Dashboard;

/// <summary>
/// Basic dashboard with counts from MongoDB projections.
/// </summary>
public static class DashboardHandler
{
    [WolverineGet("/backoffice/reporting/dashboard")]
    public static async Task<DashboardResponse> Handle(IMongoDatabase mongoDatabase, CancellationToken ct)
    {
        var productCollection = mongoDatabase.GetCollection<BsonDocument>(CollectionNames.ProductSearch);
        var productCount = await productCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: ct);

        return new DashboardResponse(
            ProductCount: (int)productCount,
            GeneratedAt: DateTime.UtcNow);
    }
}
