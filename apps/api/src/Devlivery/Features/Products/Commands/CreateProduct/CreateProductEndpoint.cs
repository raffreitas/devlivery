using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResource<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        CreateProductCommand command,
        ICommandHandler<CreateProductCommand, Result<CreateProductResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return result.ToCreated(response => $"/api/products/{response.ProductId}");
    }
}