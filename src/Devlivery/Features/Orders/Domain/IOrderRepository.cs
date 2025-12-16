namespace Devlivery.Features.Orders.Domain;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);

    Task Update(Order order);

    Task Remove(Order order);

    Task<List<Order>> GetOrdersInPeriodAsync(
        DateTime start,
        DateTime end,
        CancellationToken ct = default);

    Task<bool> ExistsItemWithProductIdAsync(
        Guid productId,
        CancellationToken ct = default);
}