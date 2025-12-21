using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public static class UpdateExpenseEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{expenseId:guid}", Handle)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>>> Handle(
        Guid expenseId,
        UpdateExpenseRequest request,
        ISender sender,
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

        return result.IsSuccess
            ? result.ToNoContent()
            : result.ToBadRequest();
    }
}

public sealed record UpdateExpenseRequest(
    Guid? SubcategoryId,
    decimal? Amount,
    DateOnly? DueDate,
    string? Supplier,
    string? Description);