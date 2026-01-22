using Devlivery.Common.Errors;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionHandler(
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateCashSessionCommand, Result<CreateCashSessionResponse>>
{
    public async ValueTask<Result<CreateCashSessionResponse>> Handle(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var existingOpen = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);

        if (existingOpen is not null)
        {
            return Result.Fail<CreateCashSessionResponse>(
                new ValidationError("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo."));
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
            cashSession.Status.ToString());

        return Result.Ok(response);
    }
}