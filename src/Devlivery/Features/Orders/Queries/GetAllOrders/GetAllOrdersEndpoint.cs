using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public static class GetAllOrdersEndpoint
{
    public sealed record Request(DateTime? Start, DateTime? End);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllOrdersResponse>>>()
            .Produces<ApiResponse<List<GetAllOrdersResponse>>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<List<GetAllOrdersResponse>>>> Handle(
        DateTime? start,
        DateTime? end,
        PaymentMethod? paymentMethod,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery(start, end, paymentMethod);
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}