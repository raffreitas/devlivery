using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public static class DeleteOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>, NotFound<ApiResponse>>> Handle(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteOrderCommand(id);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                _ => result.ToBadRequest()
            };
    }
}