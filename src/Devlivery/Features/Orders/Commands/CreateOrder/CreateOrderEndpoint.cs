using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

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
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Created<ApiResponse<CreateOrderResponse>>, BadRequest<ApiResponse<CreateOrderResponse>>, NotFound<ApiResponse<CreateOrderResponse>>, Conflict<ApiResponse<CreateOrderResponse>>>> Handle(
        Request request,
        ISender sender,
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

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated("/api/orders")
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}