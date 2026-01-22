using Devlivery.Common.Extensions;
using Devlivery.Infrastructure.Persistence.Context;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionsQuery, GetCashSessionsResponse[]>
{
    public async ValueTask<GetCashSessionsResponse[]> Handle(GetCashSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var sessionsQuery = dbContext.CashSessions
            .AsNoTracking()
            .AsQueryable();

        sessionsQuery = sessionsQuery
            .WhereDateInRange(cs => cs.StartAt, query.StartDate, query.EndDate);

        if (query.Status is not null)
        {
            sessionsQuery = sessionsQuery.Where(cs => cs.Status == query.Status);
        }

        var sessions = await sessionsQuery
            .OrderByDescending(cs => cs.StartAt)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => GetCashSessionsResponse.FromDomain(s))
            .ToArray();
    }
}