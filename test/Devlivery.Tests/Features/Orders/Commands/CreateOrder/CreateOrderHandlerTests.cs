using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Application.Errors;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.CreateOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateOrderHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var productId = Guid.NewGuid();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(productId, 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "11999999999",
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash,
            DeliveryFee: 5.00m,
            Notes: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Create_Order_With_Correct_Properties()
    {
        // Arrange
        var product = new Product(
            name: "Pizza Margherita",
            description: "Pizza com molho de tomate e queijo",
            price: 35.00m,
            category: "Pizzas",
            available: true,
            establishmentId: Guid.NewGuid()
        );

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(product.Id, 2, "Sem cebola")],
            CustomerName: "João Silva",
            CustomerPhone: "11987654321",
            DeliveryAddress: "Av. Paulista, 1000",
            PaymentMethod: PaymentMethod.CreditCard,
            DeliveryFee: 10.00m,
            Notes: "Entrega rápida"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await orderRepository.Received(1).AddAsync(
            Arg.Is<Order>(o =>
                o.Customer.Name == "João Silva" &&
                o.Customer.Phone != null &&
                o.Customer.Phone.Number == "11987654321" &&
                o.DeliveryAddress == "Av. Paulista, 1000" &&
                o.PaymentMethod == PaymentMethod.CreditCard &&
                o.DeliveryFee == 10.00m &&
                o.Status == OrderStatus.Pending &&
                o.Notes == "Entrega rápida"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Add_Items_To_Order()
    {
        // Arrange
        var product1 = new Product("Pizza Margherita", "Descrição", 35.00m, "Pizzas", true, Guid.NewGuid());
        var product2 = new Product("Refrigerante", "Descrição", 5.00m, "Bebidas", true, Guid.NewGuid());

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product1, product2]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items:
            [
                new OrderItemDto(product1.Id, 2, null),
                new OrderItemDto(product2.Id, 3, null)
            ],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash,
            DeliveryFee: 5.00m,
            Notes: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await orderRepository.Received(1).AddAsync(
            Arg.Is<Order>(o => o.Items.Count == 2),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Use_Product_Price_For_OrderItem()
    {
        // Arrange
        var productPrice = 50.00m;
        var product = new Product("Produto", "Descrição", productPrice, "Categoria", true, Guid.NewGuid());

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(product.Id, 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash,
            DeliveryFee: 5.00m,
            Notes: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await orderRepository.Received(1).AddAsync(
            Arg.Is<Order>(o =>
                o.Items.First().UnitPrice == productPrice &&
                o.Total == (productPrice * 2) + 5.00m), // 2 items + delivery fee
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Use_TenantId_From_TenantAccessor()
    {
        // Arrange
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, Guid.NewGuid());

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var expectedTenantId = tenantAccessor.Tenant.Id;

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(product.Id, 1, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await orderRepository.Received(1).AddAsync(
            Arg.Is<Order>(o => o.EstablishmentId == expectedTenantId),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, Guid.NewGuid());

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
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
    public async Task Handle_Should_Return_CreateOrderResponse_With_OrderId()
    {
        // Arrange
        var product = new Product("Produto", "Descrição", 10.00m, "Categoria", true, Guid.NewGuid());

        var productRepository = Substitute.For<IProductRepository>();
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var handler = new CreateOrderHandler(orderRepository, productRepository, unitOfWork, tenantAccessor);

        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(product.Id, 1, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Rua Teste, 123",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.OrderId.ShouldNotBe(Guid.Empty);
    }
}