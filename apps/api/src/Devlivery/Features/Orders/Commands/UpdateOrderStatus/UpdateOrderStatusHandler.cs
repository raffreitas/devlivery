using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICashSessionRepository cashSessionRepository) : ICommandHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail(new NotFoundError("Pedido não encontrado"));

        if (command.Status == OrderStatus.Delivered && order.Status != OrderStatus.Delivered &&
            await cashSessionRepository.GetActiveSessionAsync(cancellationToken) is null)
            return Result.Fail(new ValidationError("Abra o caixa antes de marcar o pedido como entregue."));

        order.UpdateStatus(command.Status);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
