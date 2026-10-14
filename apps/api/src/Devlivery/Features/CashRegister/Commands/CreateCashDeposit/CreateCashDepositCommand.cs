using FluentValidation;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed record CreateCashDepositCommand(
    Guid CashSessionId,
    decimal Amount,
    string? Notes
);

public sealed class CreateCashDepositValidator : AbstractValidator<CreateCashDepositCommand>
{
    public CreateCashDepositValidator()
    {
        RuleFor(x => x.CashSessionId).NotEmpty().WithName("Caixa");
        RuleFor(x => x.Amount).GreaterThan(0).WithName("Valor do aporte");
    }
}