using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<GetProductByIdResponse>> HandleAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .ForTenant(tenantAccessor.Tenant.Id)
            .AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new GetProductByIdResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.Available,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? Result.Fail("Produto não encontrado") : Result.Ok(product);
    }
}