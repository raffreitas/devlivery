using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<List<GetAllProductsResponse>>> HandleAsync(
        GetAllProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new GetAllProductsResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.Available,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result.Ok(products);
    }
}