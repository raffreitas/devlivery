using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.Orders.Commands.UpdateOrder;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrder;

[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateOrderHandler _handler;

    public UpdateOrderHandlerTests()
    {
        _handler = new UpdateOrderHandler(_orderRepository, _productRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentOrder_WhenUpdating_ThenReturnsNotFoundError()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var command = CreateCommand(Guid.NewGuid(), [new OrderItemDto(Guid.NewGuid(), 1, null)], []);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Canceled)]
    public async Task GivenFinalizedOrder_WhenUpdating_ThenReturnsValidationErrorWithoutCommitting(
        OrderStatus status)
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(status).Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var command = CreateCommand(order.Id, [new OrderItemDto(Guid.NewGuid(), 1, null)], []);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<ValidationError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenMissingProduct_WhenUpdating_ThenReturnsNotFoundErrorAndKeepsItems()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var originalItems = order.Items.ToList();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);
        var command = CreateCommand(order.Id, [new OrderItemDto(Guid.NewGuid(), 1, null)], []);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        order.Items.ShouldBe(originalItems);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenItemWhoseProductWasPausedAfterOrdering_WhenUpdating_ThenKeepsItemAndAppliesChanges()
    {
        // Arrange
        var pausedProduct = new ProductBuilder().WithPrice(40m).AsUnavailable().Build();
        var order = new OrderBuilder()
            .WithItems(new OrderItemBuilder().WithProduct(pausedProduct).WithQuantity(1).Build())
            .Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([pausedProduct]);
        var command = CreateCommand(order.Id, [new OrderItemDto(pausedProduct.Id, 1, null)], []) with
        {
            DeliveryAddress = "Av. Brasil, 500"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.DeliveryAddress.FullAddress.ShouldBe("Av. Brasil, 500");
        order.Items.ShouldHaveSingleItem().ProductId.ShouldBe(pausedProduct.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenUnavailableProduct_WhenUpdating_ThenReturnsValidationErrorNamingTheProduct()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var unavailable = new ProductBuilder().WithName("Pizza Doce").AsUnavailable().Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([unavailable]);
        var command = CreateCommand(order.Id, [new OrderItemDto(unavailable.Id, 1, null)], []);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<ValidationError>().Message.ShouldContain("Pizza Doce");
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenOpenOrder_WhenUpdating_ThenReplacesDetailsItemsAndPaymentsAndCommits()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).Build();
        var order = new OrderBuilder().WithCustomPayments([payment]).Build();
        var product = new ProductBuilder().WithPrice(45.50m).Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()).Returns([product]);

        var command = new UpdateOrderCommand(
            Id: order.Id,
            Items: [new OrderItemDto(product.Id, 2, "Bem passada")],
            CustomerName: "Maria Souza",
            CustomerPhone: "21912345678",
            DeliveryAddress: "Av. Brasil, 500",
            Payments: [new OrderPaymentDto(payment.Id, PaymentMethod.Pix, 101m)],
            DeliveryFee: 10m,
            DeliveryReference: "Casa 2",
            Notes: "Troco para 200");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Customer.Name.ShouldBe("Maria Souza");
        order.DeliveryAddress.FullAddress.ShouldBe("Av. Brasil, 500");
        order.DeliveryAddress.Reference.ShouldBe("Casa 2");
        order.Notes.ShouldBe("Troco para 200");
        var item = order.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(product.Id);
        item.UnitPrice.ShouldBe(45.50m);
        item.EstablishmentId.ShouldBe(order.EstablishmentId);
        order.Total.ShouldBe(101m);
        payment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        payment.Amount.ShouldBe(101m);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    private static UpdateOrderCommand CreateCommand(Guid orderId, OrderItemDto[] items, OrderPaymentDto[] payments)
    {
        return new UpdateOrderCommand(orderId, items, "João Silva", null, "Rua das Flores, 123", payments);
    }
}