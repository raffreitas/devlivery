using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed class GetActiveCashSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cash-register/sessions/active", Handle)
            .WithTags(Tags.CashRegister)
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