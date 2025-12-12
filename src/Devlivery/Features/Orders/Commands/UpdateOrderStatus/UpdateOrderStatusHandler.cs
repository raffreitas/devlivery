using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(ApplicationDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
            return Result.Fail("Pedido não encontrado");

        if (!Enum.TryParse<OrderStatus>(command.Status, ignoreCase: true, out var status))
            return Result.Fail("Status inválido");

        order.UpdateStatus(status);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}