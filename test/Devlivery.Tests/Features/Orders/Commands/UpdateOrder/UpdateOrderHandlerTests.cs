using Devlivery.Features.Orders.Commands.UpdateOrder;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Application.Errors;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var handler = new UpdateOrderHandler(orderRepository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Return_DomainRuleError_When_Order_Is_Canceled()
    {
        // Arrange
        var order = fixture.CreateOrder(status: OrderStatus.Canceled);

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderHandler(orderRepository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(
            Id: order.Id,
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is DomainRuleError);
    }

    [Fact]
    public async Task Handle_Should_Return_DomainRuleError_When_Order_Is_Delivered()
    {
        // Arrange
        var order = fixture.CreateOrder(status: OrderStatus.Delivered);

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderHandler(orderRepository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(
            Id: order.Id,
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is DomainRuleError);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Product_Does_Not_Exist()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]); // Retorna lista vazia

        var handler = new UpdateOrderHandler(orderRepository, productRepository, unitOfWork);

        var command = new UpdateOrderCommand(
            Id: order.Id,
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Replace_Order_Items()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var product = new Product("Novo Produto", "Descrição", 25.00m, "Categoria", true, order.EstablishmentId);

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
            Items: [new OrderItemDto(product.Id, 3, "Observação")],
            CustomerName: "Cliente Atualizado",
            CustomerPhone: "11988887777",
            DeliveryAddress: "Nova Rua, 456",
            PaymentMethod: PaymentMethod.DebitCard,
            DeliveryFee: 8.00m
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.Count.ShouldBe(1); // Deve ter apenas o novo item
        order.Items.First().ProductId.ShouldBe(product.Id);
        order.Items.First().Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_Should_Update_Order_Details()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, order.EstablishmentId);

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
            Items: [new OrderItemDto(product.Id, 1, null)],
            CustomerName: "João Atualizado",
            CustomerPhone: "11900001111",
            DeliveryAddress: "Av. Atualizada, 999",
            PaymentMethod: PaymentMethod.Pix,
            DeliveryFee: 15.00m,
            Notes: "Nova observação"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Customer.Name.ShouldBe("João Atualizado");
        order.Customer.Phone.ShouldNotBeNull();
        order.Customer.Phone.Number.ShouldBe("11900001111");
        order.DeliveryAddress.FullAddress.ShouldBe("Av. Atualizada, 999");
        order.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        order.DeliveryFee.ShouldBe(15.00m);
        order.Notes.ShouldBe("Nova observação");
    }

    [Fact]
    public async Task Handle_Should_Call_Update_On_Repository()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, order.EstablishmentId);

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
            Items: [new OrderItemDto(product.Id, 1, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await orderRepository.Received(1).Update(order);
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, order.EstablishmentId);

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
            Items: [new OrderItemDto(product.Id, 1, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
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
            Items: [new OrderItemDto(product.Id, 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash,
            DeliveryFee: 10.00m
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.First().UnitPrice.ShouldBe(productPrice);
        order.Total.ShouldBe((productPrice * 2) + 10.00m); // 2 items + delivery fee
    }
}