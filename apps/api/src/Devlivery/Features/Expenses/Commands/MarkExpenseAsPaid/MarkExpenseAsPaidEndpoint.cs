using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public static class MarkExpenseAsPaidEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{expenseId:guid}/mark-as-paid", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid expenseId,
        MarkExpenseAsPaidRequest request,
        ICommandHandler<MarkExpenseAsPaidCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new MarkExpenseAsPaidCommand(expenseId, request.PaymentDate);
        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}

public sealed record MarkExpenseAsPaidRequest(DateOnly PaymentDate);