using Bogus;

using Devlivery.Features.Expenses.Domain.Aggregates.Categories;

namespace Devlivery.Tests.Common.Builders;

public sealed class CategoryBuilder
{
    private readonly Faker _faker = new();

    private string _categoryName;
    private Guid _establishmentId;
    private bool _isActive;

    public CategoryBuilder()
    {
        _categoryName = _faker.Commerce.Categories(1)[0];
        _isActive = true;
    }

    public CategoryBuilder WithName(string name)
    {
        _categoryName = name;
        return this;
    }

    public CategoryBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public CategoryBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Category Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        var category = new Category(_categoryName, _establishmentId);

        if (!_isActive)
        {
            category.Deactivate();
        }

        return category;
    }
}

