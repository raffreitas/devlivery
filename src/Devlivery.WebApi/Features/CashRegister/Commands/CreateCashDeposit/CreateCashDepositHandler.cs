using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Features.CashRegister.Errors;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CashDepositResponse>> HandleAsync(
        CreateCashDepositCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        // Verify that the cash session exists and is open
        var cashSession = await dbContext.CashSessions
            .Include(x => x.Deposits)
            .FirstOrDefaultAsync(cs => cs.Id == command.CashSessionId, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<CashDepositResponse>(CashRegisterErrors.CashSessionNotFound);
        }

        if (cashSession.Status != CashSessionStatus.Open)
        {
            return Result.Fail<CashDepositResponse>(
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

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = CashDepositResponse.FromDomain(deposit);
        return Result.Ok(response);
    }
}