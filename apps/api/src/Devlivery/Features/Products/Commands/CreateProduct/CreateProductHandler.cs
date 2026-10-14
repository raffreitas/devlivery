using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async Task<Result<CreateProductResponse>> HandleAsync(
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
        await unitOfWork.CommitAsync(cancellationToken);

        var response = new CreateProductResponse(product.Id);

        return Result.Ok(response);
    }
}