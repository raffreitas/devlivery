using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public static class UpdateOrderEndpoint
{
    internal sealed record UpdateOrderRequest(
        OrderItemDto[] Items,
        string CustomerName,
        string? CustomerPhone,
        string DeliveryAddress,
        string? DeliveryReference,
        PaymentMethod PaymentMethod,
        decimal DeliveryFee = 0,
        string? Notes = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>, NotFound<ApiResponse>, Conflict<ApiResponse>>>
        Handle(
            Guid id,
            UpdateOrderRequest request,
            ISender sender,
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
            request.DeliveryReference,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                NotFoundError => result.ToNotFound(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}