using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using FluentValidation;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    internal sealed record Request(string Name, string Description, decimal Price, string Category, bool Available);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateProductResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        Request request,
        ISender sender,
        IValidator<CreateProductCommand> validator,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Category, request.Available);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/products/{result.Value.ProductId}")
            : result.ToBadRequestProblem();
    }
}