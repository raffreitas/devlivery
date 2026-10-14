using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusEndpoint : IEndpoint
{
    internal sealed record UpdateOrderStatusRequest(OrderStatus Status);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("orders/{id:guid}/status", Handle)
            .WithTags(Tags.Orders)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateOrderStatusRequest request,
        ICommandHandler<UpdateOrderStatusCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);

        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}