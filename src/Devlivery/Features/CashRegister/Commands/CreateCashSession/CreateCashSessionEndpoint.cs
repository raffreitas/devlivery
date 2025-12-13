using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public static class CreateCashSessionEndpoint
{
    internal sealed record Request(
        Guid AttendantId,
        string AttendantName,
        decimal OpeningAmount,
        string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Created<ApiResponse<CreateCashSessionResponse>>, BadRequest<ApiResponse<CreateCashSessionResponse>>, Conflict<ApiResponse<CreateCashSessionResponse>>>> Handle(
        Request request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateCashSessionCommand(request.AttendantId, request.AttendantName, request.OpeningAmount, request.Notes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/cash-sessions/{result.Value.Id}")
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}