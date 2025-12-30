using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public static class MarkExpenseAsPaidEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{expenseId:guid}/mark-as-paid", Handle)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid expenseId, MarkExpenseAsPaidRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new MarkExpenseAsPaidCommand(expenseId, request.PaymentDate);
        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}

public sealed record MarkExpenseAsPaidRequest(DateOnly PaymentDate);