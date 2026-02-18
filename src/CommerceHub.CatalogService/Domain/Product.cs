namespace CommerceHub.CatalogService.Domain;

public sealed class Product
{
    public Guid Id { get; init; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Sku { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; init; }
    public decimal BasePrice { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }
    
    private Product() { }
    
    private Product(string name, string description, string sku, Guid categoryId, decimal basePrice, string? imageUrl)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Description = description;
        Sku = sku;
        CategoryId = categoryId;
        BasePrice = basePrice;
        ImageUrl = imageUrl;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static Product CreateNew(string name, string description, string sku, Guid categoryId, decimal basePrice, string? imageUrl = null)
    {
        return new Product(name, description, sku, categoryId, basePrice, imageUrl);
    }
    
    public void UpdateProduct(string name, string description, string sku, Guid categoryId, decimal basePrice, string? imageUrl = null, bool isActive = true)
    {
        Name = name;
        Description = description;
        Sku = sku;
        CategoryId = categoryId;
        BasePrice = basePrice;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
