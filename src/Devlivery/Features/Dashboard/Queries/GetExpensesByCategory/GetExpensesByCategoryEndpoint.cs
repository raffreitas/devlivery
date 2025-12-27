using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public static class GetExpensesByCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/expenses-by-category", Handle)
            .Produces<ApiResponse<GetExpensesByCategoryResponse>>()
            .Produces<ApiResponse<GetExpensesByCategoryResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<GetExpensesByCategoryResponse>>> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetExpensesByCategoryQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}

