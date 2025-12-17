using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

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
        GetAllOrdersHandler handler,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery(start, end, paymentMethod);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}