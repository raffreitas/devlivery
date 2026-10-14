using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Features.Orders.Commands.DeleteOrder;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.DeleteOrder;

[Trait("Category", "Unit Tests")]
public sealed class DeleteOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteOrderHandler _handler;

    public DeleteOrderHandlerTests()
    {
        _handler = new DeleteOrderHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentOrder_WhenDeleting_ThenReturnsNotFoundError()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new DeleteOrderCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExistingOrder_WhenDeleting_ThenRaisesDeletedEventRemovesAndCommits()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        var result = await _handler.HandleAsync(new DeleteOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.DomainEvents.OfType<OrderDeletedEvent>().ShouldHaveSingleItem().OrderId.ShouldBe(order.Id);
        await _orderRepository.Received(1).RemoveAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}