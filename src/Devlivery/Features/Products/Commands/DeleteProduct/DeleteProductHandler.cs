using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ApplicationDbContext dbContext) : ICommandHandler<DeleteProductCommand, Result<DeleteProductResponse>>
{
    public async ValueTask<Result<DeleteProductResponse>> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
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