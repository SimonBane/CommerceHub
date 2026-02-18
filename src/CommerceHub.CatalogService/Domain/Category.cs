namespace CommerceHub.CatalogService.Domain;

public sealed class Category
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Slug { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }
    
    private Category() { }

    private Category(string name, string slug, string? description, Guid? parentCategoryId)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Slug = slug;
        Description = description;
        ParentCategoryId = parentCategoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Category CreateNew(string name, string slug, string? description = null, Guid? parentCategoryId = null)
    {
        return new Category(name, slug, description, parentCategoryId);
    }
    
    public void UpdateCategory(string name, string slug, string? description, Guid? parentCategoryId = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, 100, nameof(name));
        
        Name = name;
        Slug = slug;
        Description = description;
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }
}
