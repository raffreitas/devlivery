using Devlivery.Common.Domain.Enums;
using Devlivery.Common.Errors;
using Devlivery.Features.Orders.Commands.UpdateOrder;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Domain;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Order_Not_Exists()
    {
        var repository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new UpdateOrderHandler(repository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(Guid.NewGuid(), Array.Empty<OrderItemDto>(), "Cliente Teste", null, "Rua Teste, 123", Array.Empty<OrderPaymentDto>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationError_When_Order_Finalized()
    {
        var order = fixture.CreateOrder(status: Devlivery.Features.Orders.Domain.Enums.OrderStatus.Delivered);
        var repository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new UpdateOrderHandler(repository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(order.Id, Array.Empty<OrderItemDto>(), "Cliente Teste", null, "Rua Teste, 123", Array.Empty<OrderPaymentDto>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is ValidationError);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Product_Missing()
    {
        var order = fixture.CreateOrder();
        var repository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        var handler = new UpdateOrderHandler(repository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(order.Id, new[] { new OrderItemDto(Guid.NewGuid(), 1, null) }, "Cliente Teste", null, "Rua Teste, 123", new[] { new OrderPaymentDto(null, PaymentMethod.Cash, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Update_Order_And_Save_When_Valid()
    {
        var order = fixture.CreateOrder();
        var repository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, order.EstablishmentId);
        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()).Returns([product]);

        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new UpdateOrderHandler(repository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(order.Id, new[] { new OrderItemDto(product.Id, 2, null) }, "Cliente Teste", null, "Rua Teste, 123", new[] { new OrderPaymentDto(null, PaymentMethod.Cash, 20m) }, 0m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_Should_Use_Product_Price_For_OrderItem()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var productPrice = 45.50m;
        var product = new Product("Produto Premium", "Descrição", productPrice, "Categoria", true,
            order.EstablishmentId);

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new UpdateOrderHandler(orderRepository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(
            Id: order.Id,
            Items: new[] { new OrderItemDto(product.Id, 2, null) },
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            DeliveryFee: 10.00m,
            Payments: Array.Empty<OrderPaymentDto>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.First().UnitPrice.ShouldBe(productPrice);
        order.Total.ShouldBe((productPrice * 2) + 10.00m); // 2 items + delivery fee
    }
}