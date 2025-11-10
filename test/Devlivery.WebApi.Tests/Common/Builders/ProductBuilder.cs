using Bogus;
using Devlivery.WebApi.Features.Products.Domain;

namespace Devlivery.WebApi.Tests.Common.Builders;

public sealed class ProductBuilder
{
    private readonly Faker _faker = new();

    private Guid _productId = Guid.CreateVersion7();
    private string _productName;
    private string _productDescription;
    private decimal _productPrice;
    private string _productCategory;
    private bool _productAvailable;

    public ProductBuilder()
    {
        _productName = _faker.Commerce.ProductName();
        _productDescription = _faker.Commerce.ProductDescription();
        _productPrice = _faker.Random.Decimal(1.0m, 999.99m);
        _productCategory = _faker.Commerce.Categories(1)[0];
        _productAvailable = _faker.Random.Bool();
    }

    public ProductBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
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

    public Product Build()
    {
        return new Product
        {
            Id = _productId,
            Name = _productName,
            Description = _productDescription,
            Price = _productPrice,
            Category = _productCategory,
            Available = _productAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}