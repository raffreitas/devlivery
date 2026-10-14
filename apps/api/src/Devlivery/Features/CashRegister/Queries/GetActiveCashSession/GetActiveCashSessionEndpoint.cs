using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/active", Handle)
            .Produces<ApiResource<GetActiveCashSessionResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        IQueryHandler<GetActiveCashSessionQuery, Result<GetActiveCashSessionResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}