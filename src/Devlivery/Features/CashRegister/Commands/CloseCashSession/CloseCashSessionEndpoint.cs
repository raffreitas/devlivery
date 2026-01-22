using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    internal sealed record CloseCashSessionRequest(decimal ClosingAmount, string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("sessions/{id:guid}/close", Handle)
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(Guid id, CloseCashSessionRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new CloseCashSessionCommand(id, request.ClosingAmount, request.Notes);

        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}