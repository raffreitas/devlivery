using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid ExpenseId);

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty().WithName("Despesa");
    }
}