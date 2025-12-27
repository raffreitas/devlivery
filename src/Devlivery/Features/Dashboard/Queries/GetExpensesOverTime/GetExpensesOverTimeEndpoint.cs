using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public static class GetExpensesOverTimeEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-over-time", Handle)
            .Produces<ApiResponse<GetExpensesOverTimeResponse>>()
            .Produces<ApiResponse<GetExpensesOverTimeResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<GetExpensesOverTimeResponse>>> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpensesOverTimeQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}