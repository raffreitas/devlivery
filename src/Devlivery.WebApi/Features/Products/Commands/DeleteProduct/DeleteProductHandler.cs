using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<DeleteProductResponse>> HandleAsync(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail("Produto não encontrado");
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(new DeleteProductResponse());
    }
}
