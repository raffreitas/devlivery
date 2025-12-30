using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public static class CreateExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateExpenseResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(CreateExpenseCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.ToApiResult(data => TypedResults.Created($"/api/expenses/{result.Value.ExpenseId}", data));
    }
}