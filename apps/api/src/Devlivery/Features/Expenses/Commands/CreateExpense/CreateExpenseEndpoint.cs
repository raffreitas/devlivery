using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("expenses", Handle)
            .WithTags(Tags.Expenses)
            .Produces<ApiResource<CreateExpenseResponse>>(StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        CreateExpenseCommand command,
        ICommandHandler<CreateExpenseCommand, Result<CreateExpenseResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        return result.ToCreated(response => $"/api/v1/expenses/{response.ExpenseId}");
    }
}