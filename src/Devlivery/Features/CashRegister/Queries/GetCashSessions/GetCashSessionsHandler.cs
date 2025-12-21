using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<List<GetCashSessionsResponse>>> HandleAsync(
        GetCashSessionsQuery query,
        CancellationToken cancellationToken = default)
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

        var response = sessions
            .Select(s => GetCashSessionsResponse.FromDomain(s))
            .ToList();

        return Result.Ok(response);
    }
}