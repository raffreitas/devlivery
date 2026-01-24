using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Infrastructure;

/// <summary>
/// Repository for Product aggregate.
/// Handles write operations and complex queries for Products.
/// </summary>
public sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Products.FindAsync([id], ct);
    }

    /// <summary>
    /// Gets multiple products by their IDs.
    /// Used when creating orders to validate and fetch products.
    /// </summary>
    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(product, ct);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public void Update(Product product)
    {
        dbContext.Products.Update(product);
    }

    /// <summary>
    /// Removes a product from the database.
    /// </summary>
    public void Remove(Product product)
    {
        dbContext.Products.Remove(product);
    }
}