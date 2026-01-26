using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .Include(x => x.Subcategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<bool> ExistsWithName(string name, Guid? parentId = null, CancellationToken ct = default)
    {
        return dbContext.ExpenseCategories
            .AnyAsync(c => c.Name == name && c.ParentCategoryId == parentId, ct);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .AsNoTracking()
            .Include(x => x.Subcategories)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await dbContext.ExpenseCategories
            .AsNoTracking()
            .Include(x => x.Subcategories)
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