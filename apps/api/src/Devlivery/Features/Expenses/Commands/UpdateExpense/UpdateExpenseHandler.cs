using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseHandler(
    IExpenseRepository expenseRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateExpenseCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateExpenseCommand command,
        CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
        {
            return Result.Fail(new NotFoundError("Despesa não encontrada."));
        }

        // Validate subcategory if changed
        if (command.CategoryId.HasValue)
        {
            var category = await categoryRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken);
            if (category is null || !category.IsActive)
            {
                return Result.Fail(new NotFoundError("Subcategoria não encontrada ou inativa."));
            }
        }

        expense.Update(
            categoryId: command.CategoryId,
            amount: command.Amount,
            dueDate: command.DueDate,
            supplier: command.Supplier,
            description: command.Description,
            paymentDate: command.PaymentDate
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
