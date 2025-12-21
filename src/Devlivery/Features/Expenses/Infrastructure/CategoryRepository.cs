using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Expenses.Infrastructure;

public sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .Include(x => x.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .AsNoTracking()
            .Include(x => x.SubCategories)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .AsNoTracking()
            .Include(x => x.SubCategories)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await dbContext.ExpenseCategories.AddAsync(category, ct);
    }

    public Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        dbContext.ExpenseCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Category category, CancellationToken ct = default)
    {
        dbContext.ExpenseCategories.Remove(category);
        return Task.CompletedTask;
    }
}