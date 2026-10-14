using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
            return Result.Fail(new NotFoundError("Produto não encontrado."));

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

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}