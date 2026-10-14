using Bogus;

using Devlivery.Domain.Aggregates.Expenses;

namespace Devlivery.Tests.Common.Builders;

public sealed class CategoryBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private string _name;
    private Guid _establishmentId;
    private bool _isActive;
    private Category? _parent;

    public CategoryBuilder()
    {
        _name = _faker.Commerce.Categories(1)[0];
        _establishmentId = Guid.NewGuid();
        _isActive = true;
    }

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public CategoryBuilder WithParent(Category parent)
    {
        _parent = parent;
        return this;
    }

    public CategoryBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    public Category Build()
    {
        var category = new Category(_name, _establishmentId);

        _parent?.AddSubcategory(category);

        if (!_isActive)
            category.Deactivate();

        return category;
    }
}