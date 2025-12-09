using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<List<CashSessionResponse>>> HandleAsync(
        GetCashSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var sessionsQuery = dbContext.CashSessions
            .ForTenant(tenantId)
            .AsNoTracking()
            .AsQueryable();

        if (query.StartDate.HasValue)
        {
            var startUtc = query.StartDate.Value.ToBrazilStartOfDayUtc();
            sessionsQuery = sessionsQuery.Where(cs => cs.StartAt >= startUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusiveUtc = query.EndDate.Value.ToBrazilEndOfDayExclusiveUtc();
            sessionsQuery = sessionsQuery.Where(cs => cs.StartAt < endExclusiveUtc);
        }

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