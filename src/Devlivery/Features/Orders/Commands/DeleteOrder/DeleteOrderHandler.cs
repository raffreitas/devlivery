using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteOrderCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Fail("Pedido não encontrado");
        }

        orderRepository.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}