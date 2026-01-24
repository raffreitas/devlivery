using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.Available,
            tenantAccessor.Tenant.Id
        );

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateProductResponse(product.Id);

        return Result.Ok(response);
    }
}