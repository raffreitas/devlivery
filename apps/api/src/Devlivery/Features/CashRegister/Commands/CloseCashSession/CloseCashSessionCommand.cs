using FluentValidation;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed record CloseCashSessionCommand(
    Guid Id,
    decimal ClosingAmount,
    string? Notes
);

public sealed class CloseCashSessionValidator : AbstractValidator<CloseCashSessionCommand>
{
    public CloseCashSessionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithName("Caixa");
        RuleFor(x => x.ClosingAmount).GreaterThanOrEqualTo(0).WithName("Valor de fechamento");
    }
}