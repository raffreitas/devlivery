using Bogus;

using Devlivery.Features.Products.Domain;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using NSubstitute;

namespace Devlivery.Tests.Features.Products;

public sealed class ProductsUnitTestFixture
{
    public Faker Faker { get; } = new("pt_BR");

    private readonly Guid _defaultTenantId = Guid.NewGuid();

    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }

    public IProductRepository CreateProductRepositoryMock()
    {
        return Substitute.For<IProductRepository>();
    }

    public IUnitOfWork CreateUnitOfWorkMock()
    {
        return Substitute.For<IUnitOfWork>();
    }

    public Product CreateProduct(
        string? name = null,
        string? description = null,
        decimal? price = null,
        string? category = null,
        bool? available = null,
        Guid? establishmentId = null)
    {
        return new Product(
            name ?? Faker.Commerce.ProductName(),
            description ?? Faker.Lorem.Sentence(),
            price ?? Faker.Random.Decimal(1, 1000),
            category ?? Faker.Commerce.Categories(1)[0],
            available ?? Faker.Random.Bool(),
            establishmentId ?? _defaultTenantId
        );
    }
}

[CollectionDefinition("Products Unit Tests")]
public sealed class ProductsUnitTestCollection : ICollectionFixture<ProductsUnitTestFixture>;