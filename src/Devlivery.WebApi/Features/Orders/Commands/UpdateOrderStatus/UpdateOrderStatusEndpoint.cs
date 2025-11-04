using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    public record Request(string Status);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{id:guid}/status", Handle)
            .Produces<ApiResponse<UpdateOrderStatusResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<UpdateOrderStatusResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        IValidator<UpdateOrderStatusCommand> validator,
        UpdateOrderStatusHandler handler,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);
        
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? result.ToOk("Order status updated successfully")
            : result.ToNotFoundProblem();
    }
}
