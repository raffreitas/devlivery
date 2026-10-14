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
        RuleFor(x => x.CategoryId).NotEmpty().WithName("Categoria");

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithName("Nome");
        });
    }
}