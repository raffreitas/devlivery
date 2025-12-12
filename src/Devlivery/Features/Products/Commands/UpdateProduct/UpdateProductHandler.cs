using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using FluentResults;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
            return Result.Fail("Produto não encontrado");

        product.Update(
            name: command.Name,
            description: command.Description,
            price: command.Price,
            category: command.Category
        );

        if (command.Available)
            product.SetAsAvailable();
        else
            product.SetAsUnavailable();

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}