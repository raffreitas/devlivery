using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand<Result>;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}