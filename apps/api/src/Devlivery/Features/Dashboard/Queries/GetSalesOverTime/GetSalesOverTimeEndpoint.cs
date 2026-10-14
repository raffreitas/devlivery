using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public static class GetSalesOverTimeEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales-over-time", Handle)
            .Produces<ApiResource<GetSalesOverTimeResponse>>()
            .Produces<ApiResource<GetSalesOverTimeResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? startDate,
        DateTime? endDate,
        IQueryHandler<GetSalesOverTimeQuery, Result<GetSalesOverTimeResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetSalesOverTimeQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}