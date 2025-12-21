using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Created<ApiResponse<CreateOrderResponse>>, BadRequest<ApiResponse<CreateOrderResponse>>, NotFound<ApiResponse<CreateOrderResponse>>, Conflict<ApiResponse<CreateOrderResponse>>>> Handle(
        CreateOrderCommand command,
        ISender sender,
        CancellationToken ct)
    {
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