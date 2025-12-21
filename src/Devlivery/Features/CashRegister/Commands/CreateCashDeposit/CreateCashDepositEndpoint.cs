using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

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

    private static async Task<Results<Created<ApiResponse<CreateCashDepositResponse>>,
        BadRequest<ApiResponse<CreateCashDepositResponse>>, NotFound<ApiResponse<CreateCashDepositResponse>>,
        Conflict<ApiResponse<CreateCashDepositResponse>>>> Handle(
        Guid cashSessionId,
        CreateCashDepositRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateCashDepositCommand(
            cashSessionId,
            request.AttendantId,
            request.AttendantName,
            request.Amount,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/cash-register/sessions/{cashSessionId}/deposits/{result.Value.Id}")
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}