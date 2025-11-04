namespace Devlivery.WebApi.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available,
    DateTime CreatedAt,
    DateTime UpdatedAt);
