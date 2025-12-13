using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderStatusCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid(out var errors))
        {
            return Result.Fail(errors);
        }

        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail(new NotFoundError("Pedido não encontrado"));

        order.UpdateStatus(command.Status);
        orderRepository.Update(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}