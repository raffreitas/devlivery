using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public static class GetExpensesByStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-by-status", Handle)
            .Produces<ApiResponse<GetExpensesByStatusResponse>>()
            .Produces<ApiResponse<GetExpensesByStatusResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpensesByStatusQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}