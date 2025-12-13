using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    internal sealed record Request(decimal ClosingAmount, string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{id:guid}/close", Handle)
            .Produces<ApiResponse<CloseCashSessionResponse>>()
            .Produces<ApiResponse<CloseCashSessionResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CloseCashSessionResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<CloseCashSessionResponse>>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Ok<ApiResponse<CloseCashSessionResponse>>, NotFound<ApiResponse<CloseCashSessionResponse>>, BadRequest<ApiResponse<CloseCashSessionResponse>>, Conflict<ApiResponse<CloseCashSessionResponse>>, InternalServerError>> Handle(
        Guid id,
        Request request,
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