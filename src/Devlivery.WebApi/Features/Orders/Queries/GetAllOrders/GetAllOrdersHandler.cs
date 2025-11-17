using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<List<GetAllOrdersResponse>>> HandleAsync(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = dbContext.Orders
            .ForTenant(tenantAccessor.Tenant.Id)
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        // Date filtering: Convert local Brazil time (BRT/BRST) to UTC before querying
        // Database stores all dates in UTC, but filters are expected in local time (America/Sao_Paulo)
        if (query.StartDate.HasValue)
        {
            var startUtc = query.StartDate.Value.ToBrazilStartOfDayUtc();
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusiveUtc = query.EndDate.Value.ToBrazilEndOfDayExclusiveUtc();
            ordersQuery = ordersQuery.Where(o => o.CreatedAt < endExclusiveUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(query.PaymentMethod, out var paymentMethod))
        {
            ordersQuery = ordersQuery.Where(o => o.PaymentMethod == paymentMethod);
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .ToHashSet();
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        var response = orders.Select(o => new GetAllOrdersResponse(
            o.Id,
            o.Items.Select(i => new OrderItemDto(
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
            o.CustomerName,
            o.CustomerPhone,
            o.DeliveryAddress,
            o.Status,
            o.Total,
            o.DeliveryFee,
            o.PaymentMethod.ToString(),
            o.CreatedAt,
            o.UpdatedAt)).ToList();

        return Result.Ok(response);
    }
}