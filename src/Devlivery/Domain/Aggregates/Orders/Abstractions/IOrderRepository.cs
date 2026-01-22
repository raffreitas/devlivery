namespace Devlivery.Domain.Aggregates.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);

    Task UpdateAsync(Order order, CancellationToken ct = default);

    Task RemoveAsync(Order order, CancellationToken ct = default);

    Task<List<Order>> GetOrdersInPeriodAsync(
        DateTime start,
        DateTime end,
        CancellationToken ct = default);

    Task<bool> ExistsItemWithProductIdAsync(
        Guid productId,
        CancellationToken ct = default);
}