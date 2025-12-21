using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid CategoryId,
    // TODO: Tenho que salvar o SubCategoryId na despesa para poder filtrar depois
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
            .NotEmpty().WithMessage("O campo 'Categoria' deve é obrigatório.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O campo 'Valor' deve ser maior que zero.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("A data de vencimento é obrigatória.");
    }
}