using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Features.CashRegister.Errors;
using Devlivery.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

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