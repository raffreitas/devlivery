using Bogus;

using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using NSubstitute;

namespace Devlivery.Tests.Features.Orders;

/// <summary>
/// Fixture para testes de unidade da feature Orders.
/// Fornece factory methods para criar mocks das dependências utilizando NSubstitute.
/// </summary>
public sealed class OrdersUnitTestFixture : IDisposable
{
    public Faker Faker { get; } = new("pt_BR");
    
    private readonly Guid _defaultTenantId = Guid.NewGuid();

    /// <summary>
    /// Cria um mock de ITenantAccessor com um tenant padrão.
    /// </summary>
    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }

    /// <summary>
    /// Cria um mock de IOrderRepository.
    /// </summary>
    public IOrderRepository CreateOrderRepositoryMock()
    {
        return Substitute.For<IOrderRepository>();
    }

    /// <summary>
    /// Cria um mock de IProductRepository.
    /// </summary>
    public IProductRepository CreateProductRepositoryMock()
    {
        return Substitute.For<IProductRepository>();
    }

    /// <summary>
    /// Cria um mock de IUnitOfWork.
    /// </summary>
    public IUnitOfWork CreateUnitOfWorkMock()
    {
        return Substitute.For<IUnitOfWork>();
    }

    /// <summary>
    /// Cria uma instância de Order para uso em testes.
    /// </summary>
    public Order CreateOrder(
        string? customerName = null,
        string? customerPhone = null,
        string? deliveryAddress = null,
        PaymentMethod? paymentMethod = null,
        OrderStatus? status = null,
        decimal? deliveryFee = null,
        Guid? establishmentId = null,
        string? notes = null)
    {
        return new Order(
            customerName ?? Faker.Person.FullName,
            customerPhone ?? Faker.Phone.PhoneNumber(),
            deliveryAddress ?? Faker.Address.FullAddress(),
            paymentMethod ?? Faker.PickRandom<PaymentMethod>(),
            status ?? OrderStatus.Pending,
            deliveryFee ?? Faker.Random.Decimal(0, 20),
            establishmentId ?? _defaultTenantId,
            notes ?? Faker.Lorem.Sentence()
        );
    }

    /// <summary>
    /// Cria uma instância de OrderItem para uso em testes.
    /// </summary>
    public OrderItem CreateOrderItem(
        Guid? productId = null,
        Guid? establishmentId = null,
        int? quantity = null,
        decimal? unitPrice = null,
        string? notes = null)
    {
        return new OrderItem(
            productId ?? Guid.NewGuid(),
            establishmentId ?? _defaultTenantId,
            quantity ?? Faker.Random.Int(1, 10),
            unitPrice ?? Faker.Random.Decimal(10, 200),
            notes
        );
    }

    public void Dispose()
    {
        // Cleanup se necessário
    }
}

[CollectionDefinition("Orders Unit Tests")]
public sealed class OrdersUnitTestCollection : ICollectionFixture<OrdersUnitTestFixture>;
