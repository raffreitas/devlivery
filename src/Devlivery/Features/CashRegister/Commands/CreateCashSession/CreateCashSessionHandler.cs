using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionHandler(
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateCashSessionResponse>> HandleAsync(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var existingOpen = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);

        if (existingOpen is not null)
        {
            return Result.Fail<CreateCashSessionResponse>(CashRegisterErrors.CashSessionAlreadyOpen);
        }

        var cashSession = new CashSession(
            establishmentId: tenantId,
            attendantId: command.AttendantId,
            attendantName: command.AttendantName,
            openingAmount: command.OpeningAmount,
            notes: command.Notes);

        await cashSessionRepository.AddAsync(cashSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.StartAt,
            cashSession.Status.ToString().ToLowerInvariant());

        return Result.Ok(response);
    }
}