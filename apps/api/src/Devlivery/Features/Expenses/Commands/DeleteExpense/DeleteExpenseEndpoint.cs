using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("expenses/{expenseId:guid}", Handle)
            .WithTags(Tags.Expenses)
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