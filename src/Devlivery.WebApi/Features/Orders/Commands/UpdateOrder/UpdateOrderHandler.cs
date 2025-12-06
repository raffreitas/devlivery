using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result> HandleAsync(
        UpdateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return Result.Fail("Método de pagamento inválido");

        var order = await dbContext.Orders
            .ForTenant(tenantAccessor.Tenant.Id)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
            return Result.Fail("Pedido não encontrado");

        if (order.Status is OrderStatus.Canceled or OrderStatus.Delivered)
            return Result.Fail(
                "Pedido não pode ser atualizado pois está cancelado ou já foi entregue");

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .ForTenant(tenantAccessor.Tenant.Id)
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            return Result.Fail("Um ou mais produtos não foram encontrados");

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        order.ClearItems();

        foreach (var item in command.Items)
        {
            var orderItem = new OrderItem(
                productId: item.ProductId,
                establishmentId: order.EstablishmentId,
                quantity: item.Quantity,
                unitPrice: productsDictionary[item.ProductId].Price,
                notes: item.Notes);

            order.AddItem(orderItem);
            dbContext.OrderItems.Add(orderItem);
        }

        order.UpdateDetails(
            customerName: command.CustomerName,
            customerPhone: command.CustomerPhone,
            deliveryAddress: command.DeliveryAddress,
            paymentMethod: paymentMethod,
            deliveryFee: command.DeliveryFee,
            notes: command.Notes
        );

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}