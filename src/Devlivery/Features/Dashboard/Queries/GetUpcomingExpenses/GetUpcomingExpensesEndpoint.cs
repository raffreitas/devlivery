using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public static class GetUpcomingExpensesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/upcoming-expenses", Handle)
            .Produces<ApiResponse<GetUpcomingExpensesResponse>>()
            .Produces<ApiResponse<GetUpcomingExpensesResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<GetUpcomingExpensesResponse>>> Handle(
        int days,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetUpcomingExpensesQuery(days);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}

