using Bogus;

using Devlivery.Domain.Aggregates.Products;

namespace Devlivery.Tests.Common.Builders;

public sealed class ProductBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private string _name;
    private string _description;
    private decimal _price;
    private string _category;
    private bool _available;
    private Guid _establishmentId;

    public ProductBuilder()
    {
        _name = _faker.Commerce.ProductName();
        _description = _faker.Commerce.ProductDescription();
        _price = _faker.Random.Decimal(1.0m, 999.99m);
        _category = _faker.Commerce.Categories(1)[0];
        _available = true;
        _establishmentId = Guid.NewGuid();
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithCategory(string category)
    {
        _category = category;
        return this;
    }

    public ProductBuilder AsUnavailable()
    {
        _available = false;
        return this;
    }

    public ProductBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public Product Build()
    {
        return new Product(
            name: _name,
            description: _description,
            price: _price,
            category: _category,
            available: _available,
            establishmentId: _establishmentId);
    }
}