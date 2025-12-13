using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateProductResponse>>, BadRequest<ApiResponse<CreateProductResponse>>>> Handle(
        CreateProductCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/products/{result.Value.ProductId}")
            : result.ToBadRequest();
    }
}