using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid CategoryId,
    decimal Amount,
    DateOnly DueDate,
    string? Supplier,
    string? Description,
    DateOnly? PaymentDate) : ICommand<Result<CreateExpenseResponse>>;

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O campo {PropertyName} deve ser maior que zero.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório.");
    }
}