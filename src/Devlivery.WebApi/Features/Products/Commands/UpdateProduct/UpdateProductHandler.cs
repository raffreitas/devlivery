using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<UpdateProductResponse>> HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail("Produto não encontrado");
        }

        product.Name = command.Name;
        product.Description = command.Description;
        product.Price = command.Price;
        product.Category = command.Category;
        product.Available = command.Available;
        product.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateProductResponse(
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
