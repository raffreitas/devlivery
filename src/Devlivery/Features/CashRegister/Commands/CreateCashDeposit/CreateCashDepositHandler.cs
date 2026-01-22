using Devlivery.Common.Errors;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositHandler(
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCashDepositCommand, Result<CreateCashDepositResponse>>
{
    public async ValueTask<Result<CreateCashDepositResponse>> Handle(
        CreateCashDepositCommand command,
        CancellationToken cancellationToken)
    {
        var cashSession = await cashSessionRepository.GetByIdAsync(command.CashSessionId, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<CreateCashDepositResponse>(new NotFoundError("Caixa não encontrado."));
        }

        if (cashSession.Status != CashSessionStatus.Open)
        {
            return Result.Fail<CreateCashDepositResponse>(
                new ValidationError("Não é possível adicionar aporte a um caixa fechado."));
        }

        var movement = cashSession.AddDeposit(command.Amount, command.AttendantId, command.Notes);

        await cashSessionRepository.UpdateAsync(cashSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateCashDepositResponse(
            movement.Id,
            movement.Amount,
            command.AttendantName,
            movement.CreatedAt);

        return Result.Ok(response);
    }
}