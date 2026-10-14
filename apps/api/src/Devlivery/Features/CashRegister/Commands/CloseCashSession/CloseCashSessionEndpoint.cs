using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    internal sealed record CloseCashSessionRequest(decimal ClosingAmount, string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("sessions/{id:guid}/close", Handle)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(
        Guid id,
        CloseCashSessionRequest request,
        ICommandHandler<CloseCashSessionCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new CloseCashSessionCommand(id, request.ClosingAmount, request.Notes);
        var result = await handler.HandleAsync(command, ct);
        return result.ToNoContent();
    }
}