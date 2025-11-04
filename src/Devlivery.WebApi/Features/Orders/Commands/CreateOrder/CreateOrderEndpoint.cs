using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateOrderResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        CreateOrderCommand request,
        IValidator<CreateOrderCommand> validator,
        CreateOrderHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(request, ct);

        return result.IsSuccess
            ? result.ToCreated("/api/orders", "Order created successfully")
            : result.ToBadRequestProblem();
    }
}