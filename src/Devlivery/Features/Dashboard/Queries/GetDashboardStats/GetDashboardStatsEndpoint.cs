using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public static class GetDashboardStatsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stats", Handle)
            .Produces<ApiResponse<GetDashboardStatsResponse>>()
            .Produces<ApiResponse<GetDashboardStatsResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(DateTime? startDate, DateTime? endDate, ISender sender,
        CancellationToken ct)
    {
        var query = new GetDashboardStatsQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}