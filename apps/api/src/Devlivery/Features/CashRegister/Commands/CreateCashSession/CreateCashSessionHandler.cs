using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionHandler(
    ICashSessionRepository cashSessionRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateCashSessionCommand, Result<CreateCashSessionResponse>>
{
    public async ValueTask<Result<CreateCashSessionResponse>> Handle(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        var tenantId = tenantAccessor.Tenant.Id;

        var existingOpen = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);

        if (existingOpen is not null)
        {
            return Result.Fail<CreateCashSessionResponse>(
                new ValidationError("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo."));
        }

        var cashSession = new CashSession(
            establishmentId: tenantId,
            attendantId: actor.Id,
            attendantName: actor.Name,
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