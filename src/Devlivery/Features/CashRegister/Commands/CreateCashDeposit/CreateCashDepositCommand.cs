using FluentValidation;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed record CreateCashDepositCommand(
    Guid CashSessionId,
    Guid AttendantId,
    string AttendantName,
    decimal Amount,
    string? Notes
);

public sealed class CreateCashDepositValidator : AbstractValidator<CreateCashDepositCommand>
{
    public CreateCashDepositValidator()
    {
        RuleFor(x => x.CashSessionId)
            .NotEmpty().WithMessage("A sessão de caixa é obrigatória.");

        RuleFor(x => x.AttendantId)
            .NotEmpty().WithMessage("O atendente é obrigatório.");

        RuleFor(x => x.AttendantName)
            .NotEmpty().WithMessage("O nome do atendente é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do atendente deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor do aporte deve ser maior que zero.");
    }
}
