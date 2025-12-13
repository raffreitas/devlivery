using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    internal sealed record Request(OrderStatus Status);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{id:guid}/status", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>, NotFound<ApiResponse>, Conflict<ApiResponse>>> Handle(
        Guid id,
        Request request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}