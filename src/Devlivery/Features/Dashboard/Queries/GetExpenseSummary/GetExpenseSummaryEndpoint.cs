using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpenseSummary;

public static class GetExpenseSummaryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expense-summary", Handle)
            .Produces<ApiResponse<GetExpenseSummaryResponse>>()
            .Produces<ApiResponse<GetExpenseSummaryResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<GetExpenseSummaryResponse>>> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpenseSummaryQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}