using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Presentation.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Products.Queries.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetProductByIdResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetProductByIdResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        IValidator<GetProductByIdQuery> validator,
        GetProductByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);

        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Product retrieved successfully")
            : result.ToNotFoundProblem();
    }
}