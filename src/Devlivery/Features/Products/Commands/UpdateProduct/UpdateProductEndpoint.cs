using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public static class UpdateProductEndpoint
{
    internal sealed record UpdateProductRequest(
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid id, UpdateProductRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.Available);

        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}