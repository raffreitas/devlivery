using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateCategory;

public sealed class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : ICommandHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    public async ValueTask<Result<CreateCategoryResponse>> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var establishmentId = tenantAccessor.Tenant.Id;

        var existingCategory = await categoryRepository
            .ExistsWithName(command.Name, command.ParentCategoryId, cancellationToken);

        if (existingCategory)
        {
            var categoryType = command.ParentCategoryId == null ? "categoria" : "subcategoria";
            return Result.Fail(new ValidationError($"Já existe uma {categoryType} com o nome '{command.Name}'."));
        }

        Category category;
        if (command.ParentCategoryId.HasValue)
        {
            var parentCategory = await categoryRepository
                .GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is not { IsActive: true })
            {
                return Result.Fail(new NotFoundError("Categoria pai não encontrada ou inativa."));
            }

            category = new Category(command.Name, establishmentId);
            parentCategory.AddSubcategory(category);
            await categoryRepository.UpdateAsync(parentCategory, cancellationToken);
        }
        else
        {
            category = new Category(command.Name, establishmentId);
            await categoryRepository.AddAsync(category, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateCategoryResponse(category.Id));
    }
}