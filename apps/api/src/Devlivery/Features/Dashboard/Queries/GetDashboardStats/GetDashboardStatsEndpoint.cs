using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public static class GetDashboardStatsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stats", Handle)
            .Produces<ApiResource<GetDashboardStatsResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? startDate,
        DateTime? endDate,
        IQueryHandler<GetDashboardStatsQuery, Result<GetDashboardStatsResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetDashboardStatsQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}