using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;

namespace Devlivery.WebApi.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.Available
        );

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateProductResponse(product.Id);

        return Result.Ok(response);
    }
}