using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.ExpenseCategories
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsWithNameAsync(string name, Guid? parentId = null, CancellationToken ct = default) =>
        dbContext.ExpenseCategories.AnyAsync(c => c.Name == name && c.ParentCategoryId == parentId, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await dbContext.ExpenseCategories.AddAsync(category, ct);
}