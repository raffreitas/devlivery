using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Features.Orders.Commands.UpdateOrderStatus;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderStatusHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _handler = new UpdateOrderStatusHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentOrder_WhenUpdatingStatus_ThenReturnsNotFoundError()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Preparing);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenPendingOrder_WhenUpdatingStatus_ThenAppliesStatusAndCommits()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Preparing);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Preparing);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenDeliveredOrder_WhenChangingStatus_ThenThrowsDomainExceptionWithoutCommitting()
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(OrderStatus.Delivered).Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Preparing);

        // Act
        var exception = await Record.ExceptionAsync(() => _handler.HandleAsync(command, CancellationToken.None));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}