using Devlivery.Features.Orders.Commands.DeleteOrder;
using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Application.Errors;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.DeleteOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteOrderHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var handler = new DeleteOrderHandler(orderRepository, unitOfWork);

        var command = new DeleteOrderCommand(Id: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Remove_Order_When_Exists()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new DeleteOrderHandler(orderRepository, unitOfWork);

        var command = new DeleteOrderCommand(Id: order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Call_Remove_On_Repository()
    {
        // Arrange
        var order = fixture.CreateOrder();

        var orderRepository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new DeleteOrderHandler(orderRepository, unitOfWork);

        var command = new DeleteOrderCommand(Id: order.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await orderRepository.Received(1).RemoveAsync(order, Arg.Any<CancellationToken>());
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

        var handler = new DeleteOrderHandler(orderRepository, unitOfWork);

        var command = new DeleteOrderCommand(Id: order.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}