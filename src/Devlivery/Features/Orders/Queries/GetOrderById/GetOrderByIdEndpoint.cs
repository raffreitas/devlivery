using Devlivery.Shared.Presentation.Models;
using Devlivery.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetOrderByIdResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetOrderByIdResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        IValidator<GetOrderByIdQuery> validator,
        GetOrderByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);

        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Order retrieved successfully")
            : result.ToNotFoundProblem();
    }
}