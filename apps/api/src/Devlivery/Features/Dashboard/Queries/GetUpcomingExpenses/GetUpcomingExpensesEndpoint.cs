using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public sealed class GetUpcomingExpensesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/upcoming-expenses", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetUpcomingExpensesResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
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
