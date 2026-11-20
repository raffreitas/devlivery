using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Common.Validation;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Identity.Authentication;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashWithdrawal;

public sealed record CashWithdrawalResponse(Guid Id, decimal Amount, string Reason, Guid CreatedBy, DateTime CreatedAt);

public sealed class CreateCashWithdrawalHandler(
    ICashSessionRepository repository,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<CreateCashWithdrawalCommand, Result<CashWithdrawalResponse>>
{
    public async Task<Result<CashWithdrawalResponse>> HandleAsync(
        CreateCashWithdrawalCommand command, CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        var session = await repository.GetByIdAsync(command.CashSessionId, cancellationToken);
        if (session is null)
            return Result.Fail(new NotFoundError("Caixa não encontrado."));

        if (session.Status != CashSessionStatus.Open)
            return Result.Fail(new ConflictError("Não é possível registrar sangria em um caixa fechado."));
        if (command.Amount > session.ExpectedCashAmount)
            return Result.Fail(new ValidationError([
                new ValidationFailure(nameof(command.Amount), "O valor da sangria supera o dinheiro disponível.")
            ]));

        var movement = session.AddWithdrawal(command.Amount, actor.Id, command.Reason);
        // Match the timestamp precision of the persisted movement.
        var createdAt = movement.CreatedAt.AddTicks(-(movement.CreatedAt.Ticks % TimeSpan.TicksPerMicrosecond));
        return Result.Ok(new CashWithdrawalResponse(movement.Id, movement.Amount, movement.Reason!, movement.CreatedBy, createdAt));
    }
}
