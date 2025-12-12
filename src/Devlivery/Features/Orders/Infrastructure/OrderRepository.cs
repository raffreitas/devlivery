using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Infrastructure;

/// <summary>
/// Repository for Order aggregate.
/// Handles write operations and complex queries for Orders.
/// </summary>
public sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    /// <summary>
    /// Gets an order by ID, including its items.
    /// </summary>
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    /// <summary>
    /// Adds a new order to the database.
    /// </summary>
    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await dbContext.Orders.AddAsync(order, ct);
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    public void Update(Order order)
    {
        dbContext.Orders.Update(order);
    }

    /// <summary>
    /// Removes an order from the database.
    /// </summary>
    public void Remove(Order order)
    {
        dbContext.Orders.Remove(order);
    }

    /// <summary>
    /// Gets all orders in a specific time period with optional filters.
    /// Used for business analytics and reporting.
    /// </summary>
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
}
