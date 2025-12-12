using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using FluentResults;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        DeleteOrderCommand command,
        CancellationToken cancellationToken = default)
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