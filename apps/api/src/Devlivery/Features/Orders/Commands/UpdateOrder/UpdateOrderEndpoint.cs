using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderEndpoint : IEndpoint
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

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}", Handle)
            .WithTags(Tags.Orders)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateOrderRequest request,
        ICommandHandler<UpdateOrderCommand, Result> handler,
        CancellationToken ct)
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

        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}