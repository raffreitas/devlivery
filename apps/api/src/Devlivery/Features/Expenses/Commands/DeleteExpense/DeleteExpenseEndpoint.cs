using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public static class DeleteExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{expenseId:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid expenseId,
        ICommandHandler<DeleteExpenseCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteExpenseCommand(expenseId);
        var result = await handler.HandleAsync(command, ct);
        return result.ToNoContent();
    }
}