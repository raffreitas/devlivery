using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public static class DeleteExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{expenseId:guid}", Handle)
            .Produces<ApiResponse>(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, NotFound<ApiResponse>>> Handle(
        Guid expenseId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteExpenseCommand(expenseId);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.ToNotFound();
    }
}