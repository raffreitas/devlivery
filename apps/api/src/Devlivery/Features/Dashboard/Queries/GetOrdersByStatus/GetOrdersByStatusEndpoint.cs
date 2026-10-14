using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/orders-by-status", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetOrdersByStatusResponse>>()
            .Produces<ApiResource<GetOrdersByStatusResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetOrdersByStatusQuery, Result<GetOrdersByStatusResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetOrdersByStatusQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}