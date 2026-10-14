using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid CategoryId,
    decimal Amount,
    DateOnly DueDate,
    string? Supplier,
    string? Description,
    DateOnly? PaymentDate);

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithName("Categoria");
        RuleFor(x => x.Amount).GreaterThan(0).WithName("Valor");
        RuleFor(x => x.DueDate).NotEmpty().WithName("Data de vencimento");
    }
}