using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail(new NotFoundError("Pedido não encontrado"));

        order.UpdateStatus(command.Status);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}