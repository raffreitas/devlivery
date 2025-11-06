using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<DeleteOrderResponse>> HandleAsync(
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

        return Result.Ok(new DeleteOrderResponse());
    }
}
