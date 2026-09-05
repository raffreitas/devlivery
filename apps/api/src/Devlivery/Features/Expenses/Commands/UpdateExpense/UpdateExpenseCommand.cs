using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid ExpenseId,
    Guid? CategoryId,
    decimal? Amount,
    DateOnly? DueDate,
    string? Supplier,
    string? Description,
    DateOnly? PaymentDate = null) : ICommand<Result>;

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("ExpenseId é obrigatório.");

        When(x => x.PaymentDate.HasValue, () =>
        {
            RuleFor(x => x.PaymentDate!.Value)
                .NotEmpty().WithMessage("PaymentDate deve ser uma data válida.");
        });

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount!.Value)
                .GreaterThan(0).WithMessage("Amount deve ser maior que zero.");
        });

        When(x => x.DueDate is not null, () =>
        {
            RuleFor(x => x.DueDate);
        });
    }
}
