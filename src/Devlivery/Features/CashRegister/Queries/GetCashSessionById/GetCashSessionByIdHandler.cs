using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<GetCashSessionByIdResponse>> HandleAsync(
        GetCashSessionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<GetCashSessionByIdResponse>(new NotFoundError("Caixa não encontrado."))
            : Result.Ok(GetCashSessionByIdResponse.FromDomain(cashSession));
    }
}