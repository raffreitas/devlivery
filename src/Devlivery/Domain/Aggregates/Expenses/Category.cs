using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Expenses;

public sealed class Category : Entity
{
    private readonly List<Category> _subcategories = [];

    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    public IReadOnlyList<Category> Subcategories => _subcategories;

    public Category(string name, Guid establishmentId)
    {
        Name = name;
        IsActive = true;
        EstablishmentId = establishmentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSubcategory(Category subcategory)
    {
        if (_subcategories.Contains(subcategory) || subcategory.ParentCategoryId == this.Id)
            throw new InvalidOperationException("A subcategoria já está associada a esta categoria.");

        if (subcategory.ParentCategoryId.HasValue && subcategory.ParentCategoryId != this.Id)
            throw new InvalidOperationException("A subcategoria já está associada a outra categoria.");

        subcategory.ParentCategoryId = this.Id;
        _subcategories.Add(subcategory);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? name = null, bool? isActive = null)
    {
        Name = name ?? Name;
        IsActive = isActive ?? IsActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}