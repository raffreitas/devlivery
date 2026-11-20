using FluentValidation;

namespace Devlivery.Features.CashRegister.Commands.CreateCashWithdrawal;

public sealed record CreateCashWithdrawalCommand(Guid CashSessionId, decimal Amount, string Reason);

public sealed class CreateCashWithdrawalValidator : AbstractValidator<CreateCashWithdrawalCommand>
{
    public CreateCashWithdrawalValidator()
    {
        RuleFor(x => x.CashSessionId).NotEmpty().WithName("Caixa");
        RuleFor(x => x.Amount)
            .InclusiveBetween(0.01m, 9999999999999999.99m)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Informe um valor com até duas casas decimais.")
            .WithName("Valor da sangria");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000).WithName("Motivo");
    }
}
