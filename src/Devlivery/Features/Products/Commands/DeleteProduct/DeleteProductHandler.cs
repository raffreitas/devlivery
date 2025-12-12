using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler(
    ProductRepository productRepository,
    UnitOfWork unitOfWork,
    ApplicationDbContext dbContext)
{
    public async Task<Result<DeleteProductResponse>> HandleAsync(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail("Produto não encontrado");
        }

        // Verificação de uso em OrderItems (query read-only, pode usar DbContext diretamente)
        var productInUse = await dbContext.OrderItems
            .Where(i => i.ProductId == product.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (productInUse)
        {
            return Result.Fail("Não é possível excluir um produto que já foi atribuido a um pedido.");
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new DeleteProductResponse());
    }
}