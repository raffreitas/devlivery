using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionDepositsQuery, Result<GetCashSessionDepositsResponse[]>>
{
    public async ValueTask<Result<GetCashSessionDepositsResponse[]>> Handle(GetCashSessionDepositsQuery query,
        CancellationToken cancellationToken)
    {
        var deposits = await dbContext.CashSessionMovements
            .AsNoTracking()
            .Where(m => m.CashSessionId == query.CashSessionId && m.EntryType == CashSessionEntryType.Deposit)
            .OrderBy(m => m.CreatedAt)
            .ToArrayAsync(cancellationToken);

        var authorIds = deposits.Select(x => x.CreatedBy)
            .Distinct()
            .ToHashSet();

        var names = await dbContext.Users
            .AsNoTracking()
            .Where(x => authorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Result.Ok(deposits.Select(x => GetCashSessionDepositsResponse.FromDomain(
            x, names.GetValueOrDefault(x.CreatedBy, "Usuário indisponível"))).ToArray());
    }
}