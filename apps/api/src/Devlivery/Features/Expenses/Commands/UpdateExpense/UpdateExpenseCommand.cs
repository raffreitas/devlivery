using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid ExpenseId,
    Guid? CategoryId,
    decimal? Amount,
    DateOnly? DueDate,
    string? Supplier,
    string? Description,
    DateOnly? PaymentDate = null);

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty().WithName("Despesa");

        RuleFor(x => x.Amount).GreaterThan(0).WithName("Valor");

        RuleFor(x => x.PaymentDate)
            .NotEqual(DateOnly.MinValue)
            .WithMessage("O campo '{PropertyName}' deve ser uma data válida.")
            .WithName("Data de pagamento");
    }
}