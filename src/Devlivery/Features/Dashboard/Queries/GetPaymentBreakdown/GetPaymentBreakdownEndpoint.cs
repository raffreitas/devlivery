using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public static class GetPaymentBreakdownEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/payment-breakdown", Handle)
            .Produces<ApiResponse<GetPaymentBreakdownResponse>>()
            .Produces<ApiResponse<GetPaymentBreakdownResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(DateTime? startDate, DateTime? endDate, ISender sender,
        CancellationToken ct)
    {
        var query = new GetPaymentBreakdownQuery(startDate, endDate);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}