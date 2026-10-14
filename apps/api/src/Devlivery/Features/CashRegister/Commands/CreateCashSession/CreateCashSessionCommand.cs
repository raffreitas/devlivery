using FluentValidation;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed record CreateCashSessionCommand(
    decimal OpeningAmount,
    string? Notes
);

public sealed class CreateCashSessionValidator : AbstractValidator<CreateCashSessionCommand>
{
    public CreateCashSessionValidator()
    {
        RuleFor(x => x.OpeningAmount).GreaterThanOrEqualTo(0).WithName("Valor de abertura");
    }
}