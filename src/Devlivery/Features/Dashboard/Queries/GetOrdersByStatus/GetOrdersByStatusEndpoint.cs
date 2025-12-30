using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public static class GetOrdersByStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders-by-status", Handle)
            .Produces<ApiResponse<GetOrdersByStatusResponse>>()
            .Produces<ApiResponse<GetOrdersByStatusResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? startDate,
        DateTime? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetOrdersByStatusQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}