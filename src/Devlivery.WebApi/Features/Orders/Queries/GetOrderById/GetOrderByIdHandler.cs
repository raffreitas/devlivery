using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<GetOrderByIdResponse>> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .ForTenant(tenantAccessor.Tenant.Id)
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.Id, cancellationToken);

        if (order is null)
            return Result.Fail("Pedido não encontrado");

        var productIds = order.Items
            .Select(i => i.ProductId)
            .ToHashSet();
        var products = await dbContext.Products
            .ForTenant(tenantAccessor.Tenant.Id)
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        var response = new GetOrderByIdResponse(
            order.Id,
            order.Items.Select(i => new OrderItemDto(
                new ProductDto(
                    productsDictionary[i.ProductId].Id,
                    productsDictionary[i.ProductId].Name,
                    productsDictionary[i.ProductId].Description,
                    productsDictionary[i.ProductId].Price,
                    productsDictionary[i.ProductId].Category,
                    productsDictionary[i.ProductId].Available,
                    productsDictionary[i.ProductId].CreatedAt,
                    productsDictionary[i.ProductId].UpdatedAt),
                i.Quantity,
                i.Notes)).ToList(),
            order.CustomerName,
            order.CustomerPhone,
            order.DeliveryAddress,
            order.Notes,
            order.Status.ToString(),
            order.Total,
            order.DeliveryFee,
            order.PaymentMethod.ToString(),
            order.CreatedAt,
            order.UpdatedAt);

        return Result.Ok(response);
    }
}