using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("products/{id:guid}", Handle)
            .WithTags(Tags.Products)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(
        Guid id,
        ICommandHandler<DeleteProductCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        var result = await handler.HandleAsync(command, ct);
        return result.ToNoContent();
    }
}