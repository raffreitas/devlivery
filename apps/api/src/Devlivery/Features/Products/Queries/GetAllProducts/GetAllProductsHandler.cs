using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetAllProductsQuery, Result<List<GetAllProductsResponse>>>
{
    public async ValueTask<Result<List<GetAllProductsResponse>>> Handle(
        GetAllProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
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