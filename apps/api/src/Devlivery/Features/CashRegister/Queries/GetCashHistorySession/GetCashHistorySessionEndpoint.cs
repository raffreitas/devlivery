using Asp.Versioning;
using Devlivery.Common.Endpoints;
using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;
using Devlivery.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashHistorySession;

public sealed record GetCashHistorySessionQuery(Guid Id);

public sealed class GetCashHistorySessionHandler(ApplicationDbContext db)
    : IQueryHandler<GetCashHistorySessionQuery, Result<CashSessionSummary>>
{
    public async Task<Result<CashSessionSummary>> HandleAsync(GetCashHistorySessionQuery query, CancellationToken cancellationToken)
    {
        var session = await db.CashSessions.AsNoTracking().Include(s => s.Movements)
            .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken);
        return session is null ? Result.Fail(new NotFoundError("Caixa não encontrado."))
            : Result.Ok(CashSessionSummary.FromDomain(session));
    }
}

public sealed class GetCashHistorySessionEndpoint : IEndpoint
{
    public ApiVersion Version => new(2, 0);
    public void MapEndpoint(IEndpointRouteBuilder app) => app.MapGet("cash-register/sessions/{id:guid}", Handle)
        .WithTags(Tags.CashRegister).Produces<ApiResource<CashSessionSummary>>();
    private static async Task<IResult> Handle(Guid id,
        IQueryHandler<GetCashHistorySessionQuery, Result<CashSessionSummary>> handler, CancellationToken ct) =>
        (await handler.HandleAsync(new(id), ct)).ToOk();
}
