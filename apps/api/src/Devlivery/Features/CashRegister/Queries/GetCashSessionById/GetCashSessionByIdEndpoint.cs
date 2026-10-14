using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cash-register/sessions/{id:guid}", Handle)
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<GetCashSessionByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IQueryHandler<GetCashSessionByIdQuery, Result<GetCashSessionByIdResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}