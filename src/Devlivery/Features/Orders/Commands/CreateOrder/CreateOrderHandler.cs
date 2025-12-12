using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Persistence.Context;
using Devlivery.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateOrderResponse>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return Result.Fail("Método de pagamento inválido");

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        if (products.Count != productIds.Count)
            return Result.Fail("Um ou mais produtos não foram encontrados");

        var order = new Order(
            customerName: command.CustomerName,
            customerPhone: command.CustomerPhone,
            deliveryAddress: command.DeliveryAddress,
            paymentMethod: paymentMethod,
            status: OrderStatus.Pending,
            deliveryFee: command.DeliveryFee,
            establishmentId: tenantAccessor.Tenant.Id,
            notes: command.Notes
        );
        foreach (var item in command.Items)
        {
            var orderItem = new OrderItem(
                productId: item.ProductId,
                establishmentId: order.EstablishmentId,
                quantity: item.Quantity,
                unitPrice: productsDictionary[item.ProductId].Price,
                notes: item.Notes);

            order.AddItem(orderItem);
        }

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateOrderResponse(order.Id);
    }
}