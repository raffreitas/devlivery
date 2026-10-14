namespace Devlivery.Domain.Aggregates.Orders.Abstractions;

/// <summary>
/// Orders returned by this repository are tracked: changes are persisted by <c>IUnitOfWork.SaveChangesAsync</c>.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsItemWithProductIdAsync(Guid productId, CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);

    Task RemoveAsync(Order order, CancellationToken ct = default);
}