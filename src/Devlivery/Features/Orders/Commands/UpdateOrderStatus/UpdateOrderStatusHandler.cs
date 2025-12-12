using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using FluentResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(
    OrderRepository orderRepository,
    UnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail("Pedido não encontrado");

        if (!Enum.TryParse<OrderStatus>(command.Status, ignoreCase: true, out var status))
            return Result.Fail("Status inválido");

        order.UpdateStatus(status);
        orderRepository.Update(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}