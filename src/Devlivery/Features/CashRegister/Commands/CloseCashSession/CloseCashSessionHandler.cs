using Devlivery.Common.Errors;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandler(ICashSessionRepository cashSessionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CloseCashSessionCommand, Result>
{
    public async ValueTask<Result> Handle(CloseCashSessionCommand command,
        CancellationToken cancellationToken)
    {
        var cashSession = await cashSessionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail(new NotFoundError("Caixa não encontrado."));
        }

        if (cashSession.Status == CashSessionStatus.Closed)
        {
            return Result.Fail(new ValidationError("O caixa já está fechado."));
        }

        cashSession.Close(command.ClosingAmount, command.Notes);

        await cashSessionRepository.UpdateAsync(cashSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}