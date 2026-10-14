using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("orders/{id:guid}", Handle)
            .WithTags(Tags.Orders)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        ICommandHandler<DeleteOrderCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteOrderCommand(id);

        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}