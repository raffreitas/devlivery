using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public static class CreateExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateExpenseResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async
        Task<Results<Created<ApiResponse<CreateExpenseResponse>>, BadRequest<ApiResponse<CreateExpenseResponse>>>>
        Handle(
            CreateExpenseCommand command,
            ISender sender,
            CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/expenses/{result.Value.ExpenseId}")
            : result.ToBadRequest();
    }
}