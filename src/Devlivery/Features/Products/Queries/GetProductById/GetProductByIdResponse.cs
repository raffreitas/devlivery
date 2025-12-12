namespace Devlivery.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available,
    DateTime CreatedAt,
    DateTime UpdatedAt);