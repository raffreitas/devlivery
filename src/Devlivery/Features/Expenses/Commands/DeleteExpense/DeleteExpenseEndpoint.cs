using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public static class DeleteExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{expenseId:guid}", Handle)
            .Produces<ApiResponse>(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid expenseId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteExpenseCommand(expenseId);
        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}