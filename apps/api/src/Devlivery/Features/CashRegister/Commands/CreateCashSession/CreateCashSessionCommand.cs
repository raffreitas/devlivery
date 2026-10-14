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
        RuleFor(x => x.OpeningAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de abertura deve ser maior ou igual a zero.");
    }
}