using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public static class GetSalesOverTimeEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales-over-time", Handle)
            .Produces<ApiResponse<GetSalesOverTimeResponse>>()
            .Produces<ApiResponse<GetSalesOverTimeResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(DateTime? startDate, DateTime? endDate, ISender sender,
        CancellationToken ct)
    {
        var query = new GetSalesOverTimeQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}