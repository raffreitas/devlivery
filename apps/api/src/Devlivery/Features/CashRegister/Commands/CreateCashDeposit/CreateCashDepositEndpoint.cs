using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public static class CreateCashDepositEndpoint
{
    internal sealed record CreateCashDepositRequest(
        decimal Amount,
        string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sessions/{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResource<CreateCashDepositResponse>>(StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(Guid cashSessionId, CreateCashDepositRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new CreateCashDepositCommand(
            cashSessionId,
            request.Amount,
            request.Notes);
        var result = await sender.Send(command, ct);
        return result.ToCreated(response => $"/api/cash-register/sessions/{cashSessionId}/deposits/{response.Id}");
    }
}