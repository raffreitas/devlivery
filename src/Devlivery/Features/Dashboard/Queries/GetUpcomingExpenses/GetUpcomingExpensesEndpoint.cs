using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public static class GetUpcomingExpensesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/upcoming-expenses", Handle)
            .Produces<ApiResponse<GetUpcomingExpensesResponse>>()
            .Produces<ApiResponse<GetUpcomingExpensesResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(int days, ISender sender, CancellationToken ct)
    {
        var query = new GetUpcomingExpensesQuery(days);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}