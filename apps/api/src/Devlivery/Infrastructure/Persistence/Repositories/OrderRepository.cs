using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<bool> ExistsItemWithProductIdAsync(Guid productId, CancellationToken ct = default) =>
        dbContext.OrderItems.AnyAsync(oi => oi.ProductId == productId, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await dbContext.Orders.AddAsync(order, ct);

    public Task RemoveAsync(Order order, CancellationToken ct = default)
    {
        dbContext.Orders.Remove(order);
        return Task.CompletedTask;
    }
}