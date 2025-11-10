using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(ApplicationDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Fail("Pedido não encontrado");
        }

        order.Status = command.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}