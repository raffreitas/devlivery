namespace Devlivery.Domain.Aggregates.Expenses.Abstractions;

/// <summary>
/// Categories returned by this repository are tracked: changes are persisted by <c>IUnitOfWork.SaveChangesAsync</c>.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsWithNameAsync(string name, Guid? parentId = null, CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);
}