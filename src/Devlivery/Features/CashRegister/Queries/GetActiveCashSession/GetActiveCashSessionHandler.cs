using Devlivery.Common.Errors;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed class GetActiveCashSessionHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetActiveCashSessionQuery, Result<GetActiveCashSessionResponse>>
{
    public async ValueTask<Result<GetActiveCashSessionResponse>> Handle(GetActiveCashSessionQuery query,
        CancellationToken cancellationToken)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .Include(cs => cs.Movements)
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        return cashSession is null
            ? Result.Fail<GetActiveCashSessionResponse>(new NotFoundError("Não há sessão de caixa ativa."))
            : Result.Ok(GetActiveCashSessionResponse.FromDomain(cashSession));
    }
}