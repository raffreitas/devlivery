using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Products.FindAsync([id], ct);

    public Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default) =>
        dbContext.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await dbContext.Products.AddAsync(product, ct);

    public Task RemoveAsync(Product product, CancellationToken ct = default)
    {
        dbContext.Products.Remove(product);
        return Task.CompletedTask;
    }
}