using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<IEnumerable<CashDepositResponse>>> HandleAsync(
        GetCashSessionDepositsQuery query,
        CancellationToken cancellationToken = default)
    {
        var deposits = await dbContext.CashDeposits
            .AsNoTracking()
            .Where(cd => cd.CashSessionId == query.CashSessionId)
            .OrderBy(cd => cd.DepositedAt)
            .ToListAsync(cancellationToken);

        var responses = deposits.Select(CashDepositResponse.FromDomain).ToList();

        return Result.Ok(responses.AsEnumerable());
    }
}