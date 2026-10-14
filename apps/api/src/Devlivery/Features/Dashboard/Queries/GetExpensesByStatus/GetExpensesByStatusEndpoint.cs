using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public sealed class GetExpensesByStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/expenses-by-status", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetExpensesByStatusResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetExpensesByStatusQuery, Result<GetExpensesByStatusResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetExpensesByStatusQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}