using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Infrastructure;

public sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await dbContext.Orders.AddAsync(order, ct);
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Order order, CancellationToken ct = default)
    {
        dbContext.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public async Task<List<Order>> GetOrdersInPeriodAsync(
        DateTime start,
        DateTime end,
        CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .Where(o => o.Status != OrderStatus.Canceled)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsItemWithProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return dbContext.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.ProductId == productId, ct);
    }
}