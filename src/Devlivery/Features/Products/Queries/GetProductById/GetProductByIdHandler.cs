using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    public async ValueTask<Result<GetProductByIdResponse>> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
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

        return product is null
            ? Result.Fail<GetProductByIdResponse>(new NotFoundError("Produto não encontrado"))
            : Result.Ok(product);
    }
}