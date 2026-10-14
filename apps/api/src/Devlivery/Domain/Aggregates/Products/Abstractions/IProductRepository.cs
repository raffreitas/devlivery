namespace Devlivery.Domain.Aggregates.Products.Abstractions;

/// <summary>
/// Products returned by <see cref="GetByIdAsync"/> are tracked: changes are persisted by <c>IUnitOfWork.CommitAsync</c>.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Read-only lookup (not tracked), used to validate and price order items.
    /// </summary>
    Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    Task RemoveAsync(Product product, CancellationToken ct = default);
}