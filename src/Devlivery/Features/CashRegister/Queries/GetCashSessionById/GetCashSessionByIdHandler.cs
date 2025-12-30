using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashSessionByIdQuery, Result<GetCashSessionByIdResponse>>
{
    public async ValueTask<Result<GetCashSessionByIdResponse>> Handle(GetCashSessionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<GetCashSessionByIdResponse>(new NotFoundError("Caixa n�o encontrado."))
            : Result.Ok(GetCashSessionByIdResponse.FromDomain(cashSession));
    }
}