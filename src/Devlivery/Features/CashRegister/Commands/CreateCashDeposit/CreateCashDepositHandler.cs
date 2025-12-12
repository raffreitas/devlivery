using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositHandler(
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<GetCashSessionDepositsResponse>> HandleAsync(
        CreateCashDepositCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        // Verify that the cash session exists and is open
        var cashSession = await cashSessionRepository.GetByIdAsync(command.CashSessionId, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<GetCashSessionDepositsResponse>(CashRegisterErrors.CashSessionNotFound);
        }

        if (cashSession.Status != CashSessionStatus.Open)
        {
            return Result.Fail<GetCashSessionDepositsResponse>(
                new Error("CashSessionNotOpen")
                    .WithMetadata("message", "Não é possível adicionar aporte a um caixa fechado."));
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

        cashSessionRepository.Update(cashSession);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = GetCashSessionDepositsResponse.FromDomain(deposit);
        return Result.Ok(response);
    }
}