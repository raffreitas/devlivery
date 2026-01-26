using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(Guid id, ISender sender, CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        var result = await sender.Send(command, ct);
        return result.ToNoContent();
    }
}