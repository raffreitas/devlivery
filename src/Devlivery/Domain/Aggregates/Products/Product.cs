using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Products;

public sealed class Product : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Category { get; private set; }
    public bool Available { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Product(string name, string description, decimal price, string category, bool available,
        Guid establishmentId)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        Available = available;
        EstablishmentId = establishmentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string? name = null,
        string? description = null,
        decimal? price = null,
        string? category = null
    )
    {
        Name = name ?? Name;
        Description = description ?? Description;
        Price = price ?? Price;
        Category = category ?? Category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsAvailable()
    {
        Available = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsUnavailable()
    {
        Available = false;
        UpdatedAt = DateTime.UtcNow;
    }
}