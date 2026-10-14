using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string? Name,
    bool? IsActive);

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("O campo '{PropertyName}' não pode ser vazio.")
                .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
        });
    }
}