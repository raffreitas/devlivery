using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Persistence.Context;
using Devlivery.Shared.Tenancy;
using FluentResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
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

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateProductResponse(product.Id);

        return Result.Ok(response);
    }
}