using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Expenses.Domain.Aggregates.Categories;

public sealed class Category : Entity
{
    private readonly List<Category> _subCategories = [];

    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    public IReadOnlyList<Category> SubCategories => _subCategories;

    public Category(string name, Guid establishmentId)
    {
        Name = name;
        IsActive = true;
        EstablishmentId = establishmentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSubCategory(Category subCategory)
    {
        if (_subCategories.Contains(subCategory) || subCategory.ParentCategoryId == this.Id)
            throw new InvalidOperationException("A subcategoria já está associada a esta categoria.");

        subCategory.ParentCategoryId = this.Id;
        _subCategories.Add(subCategory);
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