using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositHandler(
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateCashDepositCommand, Result<CreateCashDepositResponse>>
{
    public async ValueTask<Result<CreateCashDepositResponse>> Handle(
        CreateCashDepositCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var cashSession = await cashSessionRepository.GetByIdAsync(command.CashSessionId, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<CreateCashDepositResponse>(new NotFoundError("Caixa n�o encontrado."));
        }

        if (cashSession.Status != CashSessionStatus.Open)
        {
            return Result.Fail<CreateCashDepositResponse>(
                new DomainRuleError("N�o � poss�vel adicionar aporte a um caixa fechado."));
        }

        // Create the deposit
        var deposit = new CashDeposit(
            cashSessionId: command.CashSessionId,
            establishmentId: tenantId,
            attendantId: command.AttendantId,
            attendantName: command.AttendantName,
            amount: command.Amount,
            notes: command.Notes);

        cashSession.AddDeposit(deposit);

        await cashSessionRepository.UpdateAsync(cashSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateCashDepositResponse(
            deposit.Id,
            deposit.Amount,
            deposit.AttendantName,
            deposit.CreatedAt);

        return Result.Ok(response);
    }
}