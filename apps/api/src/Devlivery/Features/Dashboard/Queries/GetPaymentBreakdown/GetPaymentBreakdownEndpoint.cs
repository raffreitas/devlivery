using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public static class GetPaymentBreakdownEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/payment-breakdown", Handle)
            .Produces<ApiResource<GetPaymentBreakdownResponse>>()
            .Produces<ApiResource<GetPaymentBreakdownResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateTime? startDate,
        DateTime? endDate,
        IQueryHandler<GetPaymentBreakdownQuery, Result<GetPaymentBreakdownResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetPaymentBreakdownQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}