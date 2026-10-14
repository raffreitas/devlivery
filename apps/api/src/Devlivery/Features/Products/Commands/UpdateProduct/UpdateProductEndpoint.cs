using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductEndpoint : IEndpoint
{
    internal sealed record UpdateProductRequest(
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:guid}", Handle)
            .WithTags(Tags.Products)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateProductRequest request,
        ICommandHandler<UpdateProductCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.Available);

        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}