using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public static class GetAllExpensesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResource<List<GetAllExpensesResponse>>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid? categoryId,
        [FromQuery] ExpenseDisplayStatus? status,
        [FromQuery(Name = "start")] DateOnly? startDate,
        [FromQuery(Name = "end")] DateOnly? endDate,
        IQueryHandler<GetAllExpensesQuery, Result<List<GetAllExpensesResponse>>> handler,
        CancellationToken ct)
    {
        var query = new GetAllExpensesQuery(categoryId, status, startDate, endDate);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}