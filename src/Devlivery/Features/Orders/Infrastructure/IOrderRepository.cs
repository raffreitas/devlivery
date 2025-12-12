using Devlivery.Features.Orders.Domain;

namespace Devlivery.Features.Orders.Infrastructure;

/// <summary>
/// Repository interface for Order aggregate.
/// Provides abstraction for order persistence operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by ID, including its items.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds a new order to the database.
    /// </summary>
    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Removes an order from the database.
    /// </summary>
    void Remove(Order order);

    /// <summary>
    /// Gets all orders in a specific time period with optional filters.
    /// Used for business analytics and reporting.
    /// </summary>
    Task<List<Order>> GetOrdersInPeriodAsync(
        DateTime start,
        DateTime end,
        CancellationToken ct = default);
}