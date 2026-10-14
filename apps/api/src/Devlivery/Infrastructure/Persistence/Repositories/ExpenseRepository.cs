using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRepository(ApplicationDbContext dbContext) : IExpenseRepository
{
    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default) =>
        dbContext.Expenses.AnyAsync(e => e.CategoryId == categoryId, ct);

    public async Task AddAsync(Expense expense, CancellationToken ct = default) =>
        await dbContext.Expenses.AddAsync(expense, ct);

    public Task RemoveAsync(Expense expense, CancellationToken ct = default)
    {
        dbContext.Expenses.Remove(expense);
        return Task.CompletedTask;
    }
}