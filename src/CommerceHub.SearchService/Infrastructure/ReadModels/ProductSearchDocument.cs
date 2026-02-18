using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommerceHub.SearchService.Infrastructure.ReadModels;

public sealed record ProductSearchDocument
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
    public DateTime Version { get; init; }
    public string[] SearchTerms { get; init; } = Array.Empty<string>();
}
