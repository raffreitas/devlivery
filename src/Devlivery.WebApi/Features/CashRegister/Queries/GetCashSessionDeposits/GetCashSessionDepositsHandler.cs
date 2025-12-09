using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed class GetCashSessionDepositsHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<IEnumerable<CashDepositResponse>>> HandleAsync(
        GetCashSessionDepositsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var deposits = await dbContext.CashDeposits
            .ForTenant(tenantId)
            .Where(cd => cd.CashSessionId == query.CashSessionId && cd.EstablishmentId == tenantId)
            .OrderBy(cd => cd.DepositedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var responses = deposits.Select(CashDepositResponse.FromDomain).ToList();

        return Result.Ok(responses.AsEnumerable());
    }
}
