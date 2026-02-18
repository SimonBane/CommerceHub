using CommerceHub.Contracts.Catalog;

namespace CommerceHub.SearchService.Features.Projections;

/// <summary>
/// No-op handler: SearchService does not maintain a category search collection;
/// category name is stored on product documents and updated via CategoryUpdated.
/// </summary>
public static class CategoryCreatedHandler
{
    public static Task Handle(CategoryCreated _, CancellationToken __) => Task.CompletedTask;
}
