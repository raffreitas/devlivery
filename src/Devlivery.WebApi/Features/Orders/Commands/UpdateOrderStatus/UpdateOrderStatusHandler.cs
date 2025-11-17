using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Devlivery.WebApi.Features.Orders.Domain;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result> HandleAsync(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .ForTenant(tenantAccessor.Tenant.Id)
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