using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public static class UpdateOrderEndpoint
{
    internal sealed record UpdateOrderRequest(
        OrderItemDto[] Items,
        string CustomerName,
        string? CustomerPhone,
        string DeliveryAddress,
        OrderPaymentDto[] Payments,
        string? DeliveryReference,
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

    private static async Task<IResult> Handle(Guid id, UpdateOrderRequest request, ISender sender, CancellationToken ct)
    {
        var command = new UpdateOrderCommand(
            id,
            request.Items,
            request.CustomerName,
            request.CustomerPhone,
            request.DeliveryAddress,
            request.Payments,
            request.DeliveryFee,
            request.DeliveryReference,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}