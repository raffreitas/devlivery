using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result> HandleAsync(
        DeleteOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .ForTenant(tenantAccessor.Tenant.Id)
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