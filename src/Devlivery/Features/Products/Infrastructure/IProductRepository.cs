using Devlivery.Features.Products.Domain;

namespace Devlivery.Features.Products.Infrastructure;

/// <summary>
/// Repository interface for Product aggregate.
/// Provides abstraction for product persistence operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets multiple products by their IDs.
    /// Used when creating orders to validate and fetch products.
    /// </summary>
    Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    Task AddAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    void Update(Product product);

    /// <summary>
    /// Removes a product from the database.
    /// </summary>
    void Remove(Product product);
}