using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public sealed class GetExpensesOverTimeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/expenses-over-time", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetExpensesOverTimeResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetExpensesOverTimeQuery, Result<GetExpensesOverTimeResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetExpensesOverTimeQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}