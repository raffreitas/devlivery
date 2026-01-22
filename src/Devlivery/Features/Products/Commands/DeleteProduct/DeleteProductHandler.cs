using Devlivery.Common.Errors;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Domain;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence;
using FluentResults;
using Mediator;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IOrderRepository orderRepository
) : ICommandHandler<DeleteProductCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail(new NotFoundError("Produto não encontrado"));
        }

        var productInUse = await orderRepository.ExistsItemWithProductIdAsync(product.Id, cancellationToken);

        if (productInUse)
        {
            return Result.Fail(
                new ValidationError("Não é possível excluir um produto que já foi atribuido a um pedido."));
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}