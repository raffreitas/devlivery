using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed class GetPaymentBreakdownEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/payment-breakdown", Handle)
            .WithTags(Tags.Dashboard)
            .Produces<ApiResource<GetPaymentBreakdownResponse>>()
            .Produces<ApiResource<GetPaymentBreakdownResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? startDate,
        DateOnly? endDate,
        IQueryHandler<GetPaymentBreakdownQuery, Result<GetPaymentBreakdownResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetPaymentBreakdownQuery(startDate, endDate);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}