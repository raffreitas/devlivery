using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public sealed record MarkExpenseAsPaidCommand(
    Guid ExpenseId,
    DateOnly PaymentDate);

public sealed class MarkExpenseAsPaidCommandValidator : AbstractValidator<MarkExpenseAsPaidCommand>
{
    public MarkExpenseAsPaidCommandValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty().WithName("Despesa");
        RuleFor(x => x.PaymentDate).NotEmpty().WithName("Data de pagamento");
    }
}