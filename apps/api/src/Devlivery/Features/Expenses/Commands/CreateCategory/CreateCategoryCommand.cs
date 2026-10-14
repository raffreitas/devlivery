using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    Guid? ParentCategoryId = null);

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithName("Nome");
    }
}