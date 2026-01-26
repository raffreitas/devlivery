using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteExpenseCommand command,
        CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
        {
            return Result.Fail(new NotFoundError("Despesa não encontrada."));
        }

        await expenseRepository.RemoveAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}