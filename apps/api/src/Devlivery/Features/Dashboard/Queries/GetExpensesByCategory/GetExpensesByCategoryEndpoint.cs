using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public static class GetExpensesByCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-by-category", Handle)
            .Produces<ApiResource<GetExpensesByCategoryResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetExpensesByCategoryQuery, Result<GetExpensesByCategoryResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetExpensesByCategoryQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}