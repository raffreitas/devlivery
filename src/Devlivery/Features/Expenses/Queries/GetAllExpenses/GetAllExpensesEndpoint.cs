using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public static class GetAllExpensesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllExpensesResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<List<GetAllExpensesResponse>>>> Handle(
        [FromQuery] Guid? categoryId,
        [FromQuery] ExpenseStatus? status,
        [FromQuery(Name = "start")] DateOnly? startDate,
        [FromQuery(Name = "end")] DateOnly? endDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetAllExpensesQuery(categoryId, status, startDate, endDate);
        var result = await sender.Send(query, ct);
        var response = ApiResponse<List<GetAllExpensesResponse>>.Success(result);

        return TypedResults.Ok(response);
    }
}