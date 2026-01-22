using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionDepositsQuery, GetCashSessionDepositsResponse[]>
{
    public async ValueTask<GetCashSessionDepositsResponse[]> Handle(GetCashSessionDepositsQuery query,
        CancellationToken cancellationToken)
    {
        var deposits = await dbContext.CashSessionMovements
            .AsNoTracking()
            .Where(m => m.CashSessionId == query.CashSessionId && m.EntryType == CashSessionEntryType.Deposit)
            .OrderBy(m => m.CreatedAt)
            .ToArrayAsync(cancellationToken);

        return [.. deposits.Select(GetCashSessionDepositsResponse.FromDomain)];
    }
}