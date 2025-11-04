using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentResults;

namespace Devlivery.WebApi.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Category = command.Category,
            Available = command.Available,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.Available,
            product.CreatedAt,
            product.UpdatedAt);

        return Result.Ok(response);
    }
}
