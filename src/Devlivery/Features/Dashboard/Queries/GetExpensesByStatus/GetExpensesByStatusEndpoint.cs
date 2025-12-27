using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public static class GetExpensesByStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-by-status", Handle)
            .Produces<ApiResponse<GetExpensesByStatusResponse>>()
            .Produces<ApiResponse<GetExpensesByStatusResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<GetExpensesByStatusResponse>>> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpensesByStatusQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}