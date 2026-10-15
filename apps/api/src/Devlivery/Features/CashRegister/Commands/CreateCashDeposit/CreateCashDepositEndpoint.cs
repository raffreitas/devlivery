using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositEndpoint : IEndpoint
{
    internal sealed record CreateCashDepositRequest(
        decimal Amount,
        string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("cash-register/sessions/{cashSessionId:guid}/deposits", Handle)
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<CreateCashDepositResponse>>(StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid cashSessionId,
        CreateCashDepositRequest request,
        ICommandHandler<CreateCashDepositCommand, Result<CreateCashDepositResponse>> handler,
        CancellationToken ct)
    {
        var command = new CreateCashDepositCommand(
            cashSessionId,
            request.Amount,
            request.Notes);
        var result = await handler.HandleAsync(command, ct);
        return result.ToCreated(response => $"/api/v1/cash-register/sessions/{cashSessionId}/deposits/{response.Id}");
    }
}