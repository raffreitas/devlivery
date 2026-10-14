using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.CreateOrder;

[Trait("Category", "Unit Tests")]
public sealed class CreateOrderHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_orderRepository, _productRepository, _unitOfWork,
            TestMocks.TenantAccessor(_tenantId));
    }

    [Fact]
    public async Task GivenMissingProduct_WhenCreatingOrder_ThenReturnsNotFoundErrorWithoutPersisting()
    {
        // Arrange
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);
        var command = CreateCommand([new OrderItemDto(Guid.NewGuid(), 1, null)], 10m);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenUnavailableProduct_WhenCreatingOrder_ThenReturnsValidationErrorNamingTheProduct()
    {
        // Arrange
        var unavailable = new ProductBuilder().WithName("Pizza Doce").AsUnavailable().Build();
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([unavailable]);
        var command = CreateCommand([new OrderItemDto(unavailable.Id, 1, null)], 10m);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<ValidationError>().Message.ShouldContain("Pizza Doce");
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenAvailableProducts_WhenCreatingOrder_ThenPersistsPendingOrderPricedFromCatalog()
    {
        // Arrange
        var pizza = new ProductBuilder().WithPrice(35m).Build();
        var soda = new ProductBuilder().WithPrice(5m).Build();
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([pizza, soda]);
        Order? addedOrder = null;
        await _orderRepository.AddAsync(Arg.Do<Order>(o => addedOrder = o), Arg.Any<CancellationToken>());

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(pizza.Id, 2, "Sem cebola"), new OrderItemDto(soda.Id, 3, null)],
            CustomerName: "João Silva",
            CustomerPhone: "11987654321",
            DeliveryAddress: "Av. Paulista, 1000",
            Payments: [new OrderPaymentDto(PaymentMethod.Cash, 100m)],
            DeliveryFee: 10m,
            DeliveryReference: "Apto 12",
            Notes: "Entrega rápida");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        addedOrder.ShouldNotBeNull();
        result.Value.OrderId.ShouldBe(addedOrder.Id);
        addedOrder.EstablishmentId.ShouldBe(_tenantId);
        addedOrder.Status.ShouldBe(OrderStatus.Pending);
        addedOrder.Customer.Name.ShouldBe("João Silva");
        addedOrder.Customer.Phone.ShouldNotBeNull().Number.ShouldBe("11987654321");
        addedOrder.DeliveryAddress.FullAddress.ShouldBe("Av. Paulista, 1000");
        addedOrder.DeliveryAddress.Reference.ShouldBe("Apto 12");
        addedOrder.Notes.ShouldBe("Entrega rápida");
        addedOrder.Items.Single(i => i.ProductId == pizza.Id).UnitPrice.ShouldBe(35m);
        addedOrder.Items.Single(i => i.ProductId == soda.Id).UnitPrice.ShouldBe(5m);
        addedOrder.Items.ShouldAllBe(i => i.EstablishmentId == _tenantId);
        addedOrder.Total.ShouldBe(95m);
        var payment = addedOrder.Payments.ShouldHaveSingleItem();
        payment.PaymentMethod.ShouldBe(PaymentMethod.Cash);
        payment.Amount.ShouldBe(100m);
        payment.EstablishmentId.ShouldBe(_tenantId);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    private static CreateOrderCommand CreateCommand(OrderItemDto[] items, decimal paymentAmount)
    {
        return new CreateOrderCommand(
            Items: items,
            CustomerName: "João Silva",
            CustomerPhone: null,
            DeliveryAddress: "Rua das Flores, 123",
            Payments: [new OrderPaymentDto(PaymentMethod.Pix, paymentAmount)]);
    }
}