using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public static class GetUpcomingExpensesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/upcoming-expenses", Handle)
            .Produces<ApiResource<GetUpcomingExpensesResponse>>()
            .Produces<ApiResource<GetUpcomingExpensesResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        int days,
        IQueryHandler<GetUpcomingExpensesQuery, Result<GetUpcomingExpensesResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetUpcomingExpensesQuery(days);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}