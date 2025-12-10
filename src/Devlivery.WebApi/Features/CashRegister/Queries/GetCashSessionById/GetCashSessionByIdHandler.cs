using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Features.CashRegister.Errors;
using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<CashSessionResponse>> HandleAsync(
        GetCashSessionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionNotFound)
            : Result.Ok(CashSessionResponse.FromDomain(cashSession));
    }
}