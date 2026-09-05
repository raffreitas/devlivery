using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed record CreateCashDepositCommand(
    Guid CashSessionId,
    decimal Amount,
    string? Notes
) : ICommand<Result<CreateCashDepositResponse>>;

public sealed class CreateCashDepositValidator : AbstractValidator<CreateCashDepositCommand>
{
    public CreateCashDepositValidator()
    {
        RuleFor(x => x.CashSessionId)
            .NotEmpty().WithMessage("A sessão de caixa é obrigatória.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor do aporte deve ser maior que zero.");
    }
}