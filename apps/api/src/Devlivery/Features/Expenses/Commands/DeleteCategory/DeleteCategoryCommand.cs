using FluentValidation;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId);

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithName("Categoria");
    }
}