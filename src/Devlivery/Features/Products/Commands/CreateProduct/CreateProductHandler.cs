using Devlivery.Features.Products.Domain;
using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using FluentResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    ProductRepository productRepository,
    UnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
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