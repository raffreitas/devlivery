using Devlivery.Shared.Extensions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public static class UpdateOrderEndpoint
{
    internal sealed record Request(
        OrderItemDto[] Items,
        string CustomerName,
        string? CustomerPhone,
        string DeliveryAddress,
        string PaymentMethod,
        decimal DeliveryFee = 0,
        string? Notes = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        ISender sender,
        IValidator<UpdateOrderCommand> validator,
        CancellationToken ct)
    {
        var command = new UpdateOrderCommand(
            id,
            request.Items,
            request.CustomerName,
            request.CustomerPhone,
            request.DeliveryAddress,
            request.PaymentMethod,
            request.DeliveryFee,
            request.Notes);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
            return result.ToNoContent();

        var errorMessage = result.Errors[0]?.Message ?? string.Empty;

        return errorMessage == "Pedido não encontrado"
            ? result.ToNotFoundProblem()
            : result.ToBadRequestProblem();
    }
}