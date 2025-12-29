using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    internal sealed record CloseCashSessionRequest(decimal ClosingAmount, string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("sessions/{id:guid}/close", Handle)
            .Produces<ApiResponse<CloseCashSessionResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid id,
        CloseCashSessionRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CloseCashSessionCommand(id, request.ClosingAmount, request.Notes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                DomainRuleError => result.ToConflict(),
                _ => TypedResults.InternalServerError()
            };
    }
}