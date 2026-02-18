using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommerceHub.BackofficeApi.Infrastructure.ReadModels;

/// <summary>
/// MongoDB read model for product (from product_search projection).
/// </summary>
public sealed record ProductReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public Guid CategoryId { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
