using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Application.Errors;
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
            return Result.Fail(new NotFoundError("Pedido não encontrado"));
        }

        // Raise domain event before deletion so handlers can access order data
        order.Delete();

        await orderRepository.RemoveAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}