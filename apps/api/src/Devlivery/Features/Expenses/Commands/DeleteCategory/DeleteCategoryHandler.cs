using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public sealed class DeleteCategoryHandler(
    ICategoryRepository categoryRepository,
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result.Fail(new NotFoundError("Categoria não encontrada."));
        }

        var hasActiveExpenses = await expenseRepository
            .ExistsWithCategoryAsync(category.Id, cancellationToken);

        if (hasActiveExpenses)
        {
            var categoryType = category.ParentCategoryId == null ? "categoria" : "subcategoria";
            return Result.Fail(new ValidationError(
                [$"Não é possível excluir a {categoryType} pois existem despesas ativas associadas a ela."]));
        }

        category.Deactivate();

        if (category.ParentCategoryId == null)
        {
            foreach (var subcategory in category.Subcategories)
            {
                subcategory.Deactivate();
            }
        }

        await categoryRepository.UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}