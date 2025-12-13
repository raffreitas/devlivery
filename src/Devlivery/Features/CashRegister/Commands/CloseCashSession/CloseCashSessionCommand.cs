using Devlivery.Shared.Extensions;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed record CloseCashSessionCommand(
    Guid Id,
    decimal ClosingAmount,
    string? Notes
) : ICommand<Result<CloseCashSessionResponse>>
{
    public bool IsValid(out string[] errors)
    {
        var result = new CloseCashSessionValidator().Validate(this);
        errors = result.GetErrors();
        return result.IsValid;
    }
};

public sealed class CloseCashSessionValidator : AbstractValidator<CloseCashSessionCommand>
{
    public CloseCashSessionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O id do caixa é obrigatório.");

        RuleFor(x => x.ClosingAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de fechamento deve ser maior ou igual a zero.");
    }
}