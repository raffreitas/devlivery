using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid ExpenseId) : IQuery<Result<GetExpenseByIdResponse>>;