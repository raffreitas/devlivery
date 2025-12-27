namespace Devlivery.Features.Expenses.Domain.Aggregates.Expenses;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Expense>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Expense expense, CancellationToken ct = default);
    Task UpdateAsync(Expense expense, CancellationToken ct = default);
    Task RemoveAsync(Expense expense, CancellationToken ct = default);
}