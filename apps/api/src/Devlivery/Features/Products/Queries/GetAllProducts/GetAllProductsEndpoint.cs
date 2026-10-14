using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResource<List<GetAllProductsResponse>>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        IQueryHandler<GetAllProductsQuery, Result<List<GetAllProductsResponse>>> handler,
        CancellationToken ct)
    {
        var query = new GetAllProductsQuery();
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}