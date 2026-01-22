using Bogus;

using Devlivery.Common.Domain.Enums;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Products.Domain;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Tests.Common.Builders;

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
    /// Usa o OrderBuilder com valores padrão sensatos.
    /// </summary>
    public Order CreateOrder(
        string? customerName = null,
        string? customerPhone = null,
        string? deliveryAddress = null,
        PaymentMethod? paymentMethod = null,
        OrderStatus? status = null,
        decimal? deliveryFee = null,
        Guid? establishmentId = null,
        string? notes = null,
        IEnumerable<OrderItem>? orderItems = null)
    {
        var orderBuilder = new OrderBuilder();

        if (!string.IsNullOrEmpty(customerName))
            orderBuilder.WithCustomerName(customerName);

        if (!string.IsNullOrEmpty(customerPhone))
            orderBuilder.WithCustomerPhone(customerPhone);

        if (!string.IsNullOrEmpty(deliveryAddress))
            orderBuilder.WithDeliveryAddress(deliveryAddress);

        if (paymentMethod.HasValue)
            orderBuilder.WithPaymentMethod(paymentMethod.Value);

        if (orderItems != null)
            orderBuilder.WithItems(orderItems.ToArray());

        if (!string.IsNullOrEmpty(notes))
            orderBuilder.WithNotes(notes);

        orderBuilder.WithEstablishmentId(establishmentId ?? _defaultTenantId);

        if (deliveryFee.HasValue)
            orderBuilder.WithDeliveryFee(deliveryFee.Value);

        var order = orderBuilder.Build();

        if (status.HasValue)
            order.UpdateStatus(status.Value);

        return order;
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
            productId: productId ?? Guid.NewGuid(),
            establishmentId: establishmentId ?? _defaultTenantId,
            quantity: quantity ?? Faker.Random.Int(1, 10),
            unitPrice: unitPrice ?? Faker.Random.Decimal(10, 200),
            notes: notes
        );
    }

    public void Dispose()
    {
        // Cleanup se necessário
    }
}

[CollectionDefinition("Orders Unit Tests")]
public sealed class OrdersUnitTestCollection : ICollectionFixture<OrdersUnitTestFixture>;