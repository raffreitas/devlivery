using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionByIdQuery, Result<GetCashSessionByIdResponse>>
{
    public async Task<Result<GetCashSessionByIdResponse>> HandleAsync(GetCashSessionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .Include(cs => cs.Movements)
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<GetCashSessionByIdResponse>(new NotFoundError("Caixa não encontrado."))
            : Result.Ok(GetCashSessionByIdResponse.FromDomain(cashSession));
    }
}