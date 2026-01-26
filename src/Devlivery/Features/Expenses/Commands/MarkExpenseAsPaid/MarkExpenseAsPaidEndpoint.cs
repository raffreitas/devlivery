using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public static class MarkExpenseAsPaidEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{expenseId:guid}/mark-as-paid", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid expenseId, MarkExpenseAsPaidRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new MarkExpenseAsPaidCommand(expenseId, request.PaymentDate);
        var result = await sender.Send(command, ct);

        return result.ToNoContent();
    }
}

public sealed record MarkExpenseAsPaidRequest(DateOnly PaymentDate);