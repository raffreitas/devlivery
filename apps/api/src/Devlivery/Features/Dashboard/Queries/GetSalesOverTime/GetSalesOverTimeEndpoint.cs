using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed class GetSalesOverTimeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/sales-over-time", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetSalesOverTimeResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetSalesOverTimeQuery, Result<GetSalesOverTimeResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetSalesOverTimeQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}
