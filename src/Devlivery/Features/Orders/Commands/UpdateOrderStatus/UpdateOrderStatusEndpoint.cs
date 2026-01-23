using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    internal sealed record UpdateOrderStatusRequest(OrderStatus Status);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{id:guid}/status", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(Guid id, UpdateOrderStatusRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);

        var result = await sender.Send(command, ct);

        return result.ToNoContent();
    }
}