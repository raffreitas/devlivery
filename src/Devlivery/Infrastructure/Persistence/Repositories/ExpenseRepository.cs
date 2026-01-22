using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRepository(ApplicationDbContext dbContext) : IExpenseRepository
{
    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<Expense>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Expenses
            .OrderByDescending(e => e.DueDate)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return dbContext.Expenses
            .AnyAsync(e => e.CategoryId == categoryId, ct);
    }

    public async Task AddAsync(Expense expense, CancellationToken ct = default)
    {
        await dbContext.Expenses.AddAsync(expense, ct);
    }

    public Task UpdateAsync(Expense expense, CancellationToken ct = default)
    {
        dbContext.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Expense expense, CancellationToken ct = default)
    {
        dbContext.Expenses.Remove(expense);
        return Task.CompletedTask;
    }
}