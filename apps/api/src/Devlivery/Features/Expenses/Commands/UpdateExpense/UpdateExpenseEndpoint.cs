using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("expenses/{expenseId:guid}", Handle)
            .WithTags(Tags.Expenses)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid expenseId,
        UpdateExpenseRequest request,
        ICommandHandler<UpdateExpenseCommand, Result> handler,
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

        var result = await handler.HandleAsync(command, ct);

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
