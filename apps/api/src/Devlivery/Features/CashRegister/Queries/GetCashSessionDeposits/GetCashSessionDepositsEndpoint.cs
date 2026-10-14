using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cash-register/sessions/{cashSessionId:guid}/deposits", Handle)
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<IEnumerable<GetCashSessionDepositsResponse>>>();
    }

    private static async Task<IResult> Handle(
        Guid cashSessionId,
        IQueryHandler<GetCashSessionDepositsQuery, Result<GetCashSessionDepositsResponse[]>> handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionDepositsQuery(cashSessionId);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}