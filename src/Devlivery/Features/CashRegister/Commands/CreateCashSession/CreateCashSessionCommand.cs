using FluentResults;
using FluentValidation;
using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed record CreateCashSessionCommand(
    Guid AttendantId,
    string AttendantName,
    decimal OpeningAmount,
    string? Notes
) : ICommand<Result<CreateCashSessionResponse>>;

public sealed class CreateCashSessionValidator : AbstractValidator<CreateCashSessionCommand>
{
    public CreateCashSessionValidator()
    {
        RuleFor(x => x.AttendantId)
            .NotEmpty().WithMessage("O atendente é obrigatório.");

        RuleFor(x => x.AttendantName)
            .NotEmpty().WithMessage("O nome do atendente é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do atendente deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.OpeningAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de abertura deve ser maior ou igual a zero.");
    }
}