using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public static class MarkExpenseAsPaidEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{expenseId:guid}/mark-as-paid", Handle)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>>> Handle(
        Guid expenseId,
        MarkExpenseAsPaidRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new MarkExpenseAsPaidCommand(expenseId, request.PaymentDate);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.ToBadRequest();
    }
}

public sealed record MarkExpenseAsPaidRequest(DateOnly PaymentDate);