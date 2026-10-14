namespace Devlivery.Domain.Aggregates.Expenses.Abstractions;

/// <summary>
/// Expenses returned by this repository are tracked: changes are persisted by <c>IUnitOfWork.CommitAsync</c>.
/// </summary>
public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default);

    Task AddAsync(Expense expense, CancellationToken ct = default);

    Task RemoveAsync(Expense expense, CancellationToken ct = default);
}