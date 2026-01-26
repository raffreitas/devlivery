using Bogus;

using Devlivery.Domain.Aggregates.Products;

namespace Devlivery.Tests.Common.Builders;

public sealed class ProductBuilder
{
    private readonly Faker _faker = new();

    private string _productName;
    private string _productDescription;
    private decimal _productPrice;
    private string _productCategory;
    private bool _productAvailable;
    private Guid _establishmentId;

    public ProductBuilder()
    {
        _productName = _faker.Commerce.ProductName();
        _productDescription = _faker.Commerce.ProductDescription();
        _productPrice = _faker.Random.Decimal(1.0m, 999.99m);
        _productCategory = _faker.Commerce.Categories(1)[0];
        _productAvailable = true;
    }

    public ProductBuilder WithName(string name)
    {
        _productName = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _productDescription = description;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _productPrice = price;
        return this;
    }

    public ProductBuilder WithCategory(string category)
    {
        _productCategory = category;
        return this;
    }

    public ProductBuilder WithAvailability(bool available)
    {
        _productAvailable = available;
        return this;
    }

    public ProductBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public Product Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        return new Product(
            name: _productName,
            description: _productDescription,
            price: _productPrice,
            category: _productCategory,
            available: _productAvailable,
            establishmentId: _establishmentId);
    }
}