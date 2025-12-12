using Devlivery.Shared.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

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

        var productInUse = await dbContext.OrderItems
            .Where(i => i.ProductId == product.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (productInUse)
        {
            return Result.Fail("Não é possível excluir um produto que já foi atribuido a um pedido.");
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(new DeleteProductResponse());
    }
}