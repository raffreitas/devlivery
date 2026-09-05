using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public static class UpdateExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{expenseId:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid expenseId, UpdateExpenseRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateExpenseCommand(
            expenseId,
            request.SubcategoryId,
            request.Amount,
            request.DueDate,
            request.Supplier,
            request.Description,
            request.PaymentDate
        );

        var result = await sender.Send(command, ct);

        return result.ToNoContent();
    }
}

public sealed record UpdateExpenseRequest(
    Guid? SubcategoryId,
    decimal? Amount,
    DateOnly? DueDate,
    string? Supplier,
    string? Description,
    DateOnly? PaymentDate = null);
