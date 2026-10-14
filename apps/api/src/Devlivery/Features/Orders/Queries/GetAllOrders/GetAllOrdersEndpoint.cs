using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", Handle)
            .WithTags(Tags.Orders)
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