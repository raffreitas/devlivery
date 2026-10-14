using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cash-register/sessions", Handle)
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<GetCashSessionsResponse[]>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        DateOnly? start,
        DateOnly? end,
        CashSessionStatus? status,
        IQueryHandler<GetCashSessionsQuery, Result<GetCashSessionsResponse[]>> handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionsQuery(start, end, status);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}