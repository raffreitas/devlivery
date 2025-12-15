using Devlivery.Features.CashRegister.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed class GetActiveCashSessionHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<GetActiveCashSessionResponse>> HandleAsync(
        GetActiveCashSessionQuery _,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .Include(cs => cs.Deposits)
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        return cashSession is null
            ? Result.Fail<GetActiveCashSessionResponse>(new NotFoundError("Caixa não encontrado."))
            : Result.Ok(GetActiveCashSessionResponse.FromDomain(cashSession));
    }
}