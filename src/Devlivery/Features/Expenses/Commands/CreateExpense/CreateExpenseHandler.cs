using Devlivery.Common.Errors;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseHandler(
    IExpenseRepository expenseRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateExpenseCommand, Result<CreateExpenseResponse>>
{
    public async ValueTask<Result<CreateExpenseResponse>> Handle(
        CreateExpenseCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            return Result.Fail<CreateExpenseResponse>(new NotFoundError("Categoria não encontrada ou inativa."));
        }

        var expense = new Expense(
            categoryId: command.CategoryId,
            amount: command.Amount,
            dueDate: command.DueDate,
            establishmentId: tenantAccessor.Tenant.Id,
            supplier: command.Supplier,
            description: command.Description,
            paymentDate: command.PaymentDate
        );

        await expenseRepository.AddAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateExpenseResponse(expense.Id));
    }
}