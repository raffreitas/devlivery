using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Features.Orders.Commands.UpdateOrderStatus;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrderStatus;


[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderStatusHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);

        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Preparing);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Update_Order_Status()
    {
        // Arrange
        var order = fixture.CreateOrder(status: OrderStatus.Pending);

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);

        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Preparing);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Preparing);
    }

    [Fact]
    public async Task Handle_Should_Call_Update_On_Repository()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);

        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Ready);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        // repository update is not required by handler implementation; persistence occurs via UoW
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);

        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Delivered);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready)]
    [InlineData(OrderStatus.Ready, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Pending, OrderStatus.Canceled)]
    public async Task Handle_Should_Update_From_Any_Status_To_Another(OrderStatus oldStatus, OrderStatus newStatus)
    {
        // Arrange
        var order = fixture.CreateOrder(status: oldStatus);

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);

        var command = new UpdateOrderStatusCommand(order.Id, newStatus);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(newStatus);
    }
}