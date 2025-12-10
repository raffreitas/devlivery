using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Extensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<List<CashSessionResponse>>> HandleAsync(
        GetCashSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var sessionsQuery = dbContext.CashSessions
            .AsNoTracking()
            .AsQueryable();

        sessionsQuery = sessionsQuery
            .WhereDateInRange(cs => cs.StartAt, query.StartDate, query.EndDate);

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse(query.Status, true, out CashSessionStatus status))
        {
            sessionsQuery = sessionsQuery.Where(cs => cs.Status == status);
        }

        var sessions = await sessionsQuery
            .OrderByDescending(cs => cs.StartAt)
            .ToListAsync(cancellationToken);

        var response = sessions
            .Select(s => CashSessionResponse.FromDomain(s))
            .ToList();

        return Result.Ok(response);
    }
}