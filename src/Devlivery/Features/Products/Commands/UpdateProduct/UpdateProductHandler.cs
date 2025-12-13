using Devlivery.Features.Products.Infrastructure;
using Devlivery.Features.Products.Shared;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid(out var errors))
        {
            return Result.Fail(errors);
        }

        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
            return Result.Fail(ProductErrors.ProductNotFound);

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