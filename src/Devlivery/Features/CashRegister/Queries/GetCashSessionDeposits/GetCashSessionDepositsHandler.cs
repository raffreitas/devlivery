using Devlivery.Shared.Infrastructure.Persistence.Context;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionDepositsQuery, GetCashSessionDepositsResponse[]>
{
    public async ValueTask<GetCashSessionDepositsResponse[]> Handle(GetCashSessionDepositsQuery query,
        CancellationToken cancellationToken)
    {
        var deposits = await dbContext.CashDeposits
            .AsNoTracking()
            .Where(cd => cd.CashSessionId == query.CashSessionId)
            .OrderBy(cd => cd.DepositedAt)
            .ToArrayAsync(cancellationToken);

        return deposits.Select(GetCashSessionDepositsResponse.FromDomain).ToArray();
    }
}