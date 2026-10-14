namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public sealed record GetAllExpensesQuery(
    Guid? CategoryId = null,
    ExpenseDisplayStatus? Status = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null);