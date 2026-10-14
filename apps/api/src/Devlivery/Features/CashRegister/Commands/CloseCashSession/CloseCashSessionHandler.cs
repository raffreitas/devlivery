using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandler(ICashSessionRepository cashSessionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CloseCashSessionCommand, Result>
{
    public async Task<Result> HandleAsync(CloseCashSessionCommand command,
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}