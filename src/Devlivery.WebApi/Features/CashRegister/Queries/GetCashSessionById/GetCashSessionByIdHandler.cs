using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Features.CashRegister.Errors;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CashSessionResponse>> HandleAsync(
        GetCashSessionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var cashSession = await dbContext.CashSessions
            .ForTenant(tenantId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionNotFound)
            : Result.Ok(CashSessionResponse.FromDomain(cashSession));
    }
}