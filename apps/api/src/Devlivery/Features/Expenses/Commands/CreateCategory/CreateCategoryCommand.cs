using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    Guid? ParentCategoryId = null);

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
    }
}