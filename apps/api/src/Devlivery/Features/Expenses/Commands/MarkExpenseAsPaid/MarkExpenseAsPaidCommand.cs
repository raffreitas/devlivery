using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public sealed record MarkExpenseAsPaidCommand(
    Guid ExpenseId,
    DateOnly PaymentDate);

public sealed class MarkExpenseAsPaidCommandValidator : AbstractValidator<MarkExpenseAsPaidCommand>
{
    public MarkExpenseAsPaidCommandValidator()
    {
        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("ExpenseId é obrigatório.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("PaymentDate é obrigatório.");
    }
}