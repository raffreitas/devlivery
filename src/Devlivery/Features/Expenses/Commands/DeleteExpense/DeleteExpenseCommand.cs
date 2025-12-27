using FluentResults;
using FluentValidation;
using Mediator;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid ExpenseId) : ICommand<Result>;

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("ExpenseId é obrigatório.");
    }
}
