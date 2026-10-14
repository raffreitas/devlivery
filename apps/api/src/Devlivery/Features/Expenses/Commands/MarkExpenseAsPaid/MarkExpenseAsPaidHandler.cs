using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

public sealed class MarkExpenseAsPaidHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<MarkExpenseAsPaidCommand, Result>
{
    public async Task<Result> HandleAsync(MarkExpenseAsPaidCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
        {
            return Result.Fail(new NotFoundError("Despesa não encontrada."));
        }

        expense.MarkAsPaid(command.PaymentDate);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}