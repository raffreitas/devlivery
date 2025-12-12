using Devlivery.Shared.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderHandler(ApplicationDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        DeleteOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Fail("Pedido não encontrado");
        }

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}