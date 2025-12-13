using Devlivery.Features.Products.Infrastructure;
using Devlivery.Features.Products.Shared;
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
        if (!command.IsValid(out var errors))
        {
            return Result.Fail<DeleteProductResponse>(errors);
        }

        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail<DeleteProductResponse>(ProductErrors.ProductNotFound);
        }

        // Verificação de uso em OrderItems (query read-only, pode usar DbContext diretamente)
        var productInUse = await dbContext.OrderItems
            .Where(i => i.ProductId == product.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (productInUse)
        {
            return Result.Fail<DeleteProductResponse>(ProductErrors.ProductInUse);
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new DeleteProductResponse());
    }
}