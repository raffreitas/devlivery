using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    internal sealed record Request(string Name, string Description, decimal Price, string Category, bool Available);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateProductResponse>>, BadRequest<ApiResponse<CreateProductResponse>>>> Handle(
        Request request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Category, request.Available);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/products/{result.Value.ProductId}")
            : result.ToBadRequest();
    }
}