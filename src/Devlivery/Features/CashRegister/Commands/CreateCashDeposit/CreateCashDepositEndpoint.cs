using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public static class CreateCashDepositEndpoint
{
    internal sealed record CreateCashDepositRequest(
        Guid AttendantId,
        string AttendantName,
        decimal Amount,
        string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sessions/{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResponse<CreateCashDepositResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(Guid cashSessionId, CreateCashDepositRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new CreateCashDepositCommand(
            cashSessionId,
            request.AttendantId,
            request.AttendantName,
            request.Amount,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.ToApiResult(onSuccess: data =>
            TypedResults.Created($"/api/cash-register/sessions/{cashSessionId}/deposits/{result.Value.Id}", data));
    }
}