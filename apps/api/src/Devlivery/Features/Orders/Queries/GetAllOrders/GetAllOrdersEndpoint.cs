using Devlivery.Common.Handlers;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public static class GetAllOrdersEndpoint
{
    public sealed record Request(DateTime? Start, DateTime? End);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResource<List<GetAllOrdersResponse>>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? start,
        DateTime? end,
        PaymentMethod? paymentMethod,
        IQueryHandler<GetAllOrdersQuery, Result<List<GetAllOrdersResponse>>> handler,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery(start, end, paymentMethod);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}