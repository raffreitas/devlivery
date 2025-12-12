using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using FluentValidation;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public static class CreateOrderEndpoint
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
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateOrderResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        Request request,
        ISender sender,
        IValidator<CreateOrderCommand> validator,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            request.Items,
            request.CustomerName,
            request.CustomerPhone,
            request.DeliveryAddress,
            request.PaymentMethod,
            request.DeliveryFee,
            request.Notes);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated("/api/orders")
            : result.ToBadRequestProblem();
    }
}