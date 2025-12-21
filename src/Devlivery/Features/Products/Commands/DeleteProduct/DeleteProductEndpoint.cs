using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<DeleteProductResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DeleteProductResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<DeleteProductResponse>>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse<DeleteProductResponse>>, NotFound<ApiResponse<DeleteProductResponse>>, Conflict<ApiResponse<DeleteProductResponse>>>> Handle(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);

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