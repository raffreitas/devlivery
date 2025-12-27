using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public sealed class MarkExpenseAsPaidHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<MarkExpenseAsPaidCommand, Result>
{
    public async ValueTask<Result> Handle(MarkExpenseAsPaidCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
        {
            return Result.Fail("Expense not found.");
        }

        expense.MarkAsPaid(command.PaymentDate);

        await expenseRepository.UpdateAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}