using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public static class GetTopProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/top-products", Handle)
            .Produces<ApiResponse<GetTopProductsResponse>>()
            .Produces<ApiResponse<GetTopProductsResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(DateTime? startDate, DateTime? endDate, ISender sender,
        CancellationToken ct)
    {
        var query = new GetTopProductsQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}