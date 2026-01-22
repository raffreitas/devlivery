using Devlivery.Common.Errors;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.UpdateCategory;

public sealed class UpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result.Fail(new NotFoundError("Categoria não encontrada."));
        }

        // Verifica se o nome já existe (se estiver sendo alterado)
        if (command.Name != null && command.Name != category.Name)
        {
            var existingCategory = await categoryRepository
                .ExistsWithName(command.Name, category.ParentCategoryId, cancellationToken);

            if (existingCategory)
            {
                var categoryType = category.ParentCategoryId == null ? "categoria" : "subcategoria";
                return Result.Fail(
                    new ValidationError([$"Já existe uma {categoryType} com o nome '{command.Name}'."]));
            }
        }

        category.Update(
            name: command.Name,
            isActive: command.IsActive
        );

        await categoryRepository.UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}