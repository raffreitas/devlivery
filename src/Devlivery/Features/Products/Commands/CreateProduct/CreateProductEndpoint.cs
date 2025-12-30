using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(CreateProductCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.ToApiResult(data => TypedResults.Created($"/api/products/{result.Value.ProductId}", data));
    }
}