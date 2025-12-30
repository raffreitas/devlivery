using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public static class GetExpensesOverTimeEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-over-time", Handle)
            .Produces<ApiResponse<GetExpensesOverTimeResponse>>()
            .Produces<ApiResponse<GetExpensesOverTimeResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpensesOverTimeQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}