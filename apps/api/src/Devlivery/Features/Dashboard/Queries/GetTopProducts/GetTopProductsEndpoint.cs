using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public static class GetTopProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/top-products", Handle)
            .Produces<ApiResource<GetTopProductsResponse>>()
            .Produces<ApiResource<GetTopProductsResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? startDate,
        DateTime? endDate,
        IQueryHandler<GetTopProductsQuery, Result<GetTopProductsResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetTopProductsQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}