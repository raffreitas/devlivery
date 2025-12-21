using Devlivery.Features.Expenses.Domain.Enums;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public sealed record GetAllExpensesQuery(
    Guid? CategoryId = null,
    ExpenseStatus? Status = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null) : IQuery<List<GetAllExpensesResponse>>;
