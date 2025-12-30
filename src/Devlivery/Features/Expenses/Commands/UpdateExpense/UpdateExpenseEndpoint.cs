using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public static class UpdateExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{expenseId:guid}", Handle)
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
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
            request.Description
        );

        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}

public sealed record UpdateExpenseRequest(
    Guid? SubcategoryId,
    decimal? Amount,
    DateOnly? DueDate,
    string? Supplier,
    string? Description);