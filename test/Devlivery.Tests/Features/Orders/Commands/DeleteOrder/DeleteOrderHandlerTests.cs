using Devlivery.Common.Errors;
using Devlivery.Features.Orders.Commands.DeleteOrder;
using Devlivery.Features.Orders.Domain;

using NSubstitute;
using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.DeleteOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteOrderHandlerTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Order_Not_Exists()
    {
        var repository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new DeleteOrderHandler(repository, unitOfWork);

        var command = new DeleteOrderCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Remove_Order_And_Save_When_Found()
    {
        var order = fixture.CreateOrder();
        var repository = fixture.CreateOrderRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new DeleteOrderHandler(repository, unitOfWork);

        var command = new DeleteOrderCommand(order.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).RemoveAsync(order, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}