using Asp.Versioning;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashOverview;

public sealed class GetCashOverviewEndpoint : IEndpoint
{
    public ApiVersion Version => new(2, 0);

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("cash-register/overview", Handle).WithTags(Tags.CashRegister)
            .Produces<ApiResource<GetCashOverviewResponse>>();

    private static async Task<IResult> Handle(
        IQueryHandler<GetCashOverviewQuery, Result<GetCashOverviewResponse>> handler, CancellationToken ct) =>
        (await handler.HandleAsync(new GetCashOverviewQuery(), ct)).ToOk();
}