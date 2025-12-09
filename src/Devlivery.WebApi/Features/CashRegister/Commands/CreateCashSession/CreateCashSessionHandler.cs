using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.CashRegister.Errors;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateCashSessionResponse>> HandleAsync(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var existingOpen = await dbContext.CashSessions
            .ForTenant(tenantId)
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