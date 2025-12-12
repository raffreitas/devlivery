using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Errors;
using Devlivery.Shared.Database.Context;
using Devlivery.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateCashSessionResponse>> HandleAsync(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var existingOpen = await dbContext.CashSessions
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .AnyAsync(cancellationToken);

        if (existingOpen)
        {
            return Result.Fail<CreateCashSessionResponse>(CashRegisterErrors.CashSessionAlreadyOpen);
        }

        var cashSession = new CashSession(
            establishmentId: tenantId,
            attendantId: command.AttendantId,
            attendantName: command.AttendantName,
            openingAmount: command.OpeningAmount,
            notes: command.Notes);

        dbContext.CashSessions.Add(cashSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.StartAt,
            cashSession.Status.ToString().ToLowerInvariant());

        return Result.Ok(response);
    }
}