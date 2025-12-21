namespace Devlivery.Features.Products.Queries.GetAllProducts;

public sealed record GetAllProductsResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available,
    DateTime CreatedAt,
    DateTime UpdatedAt);