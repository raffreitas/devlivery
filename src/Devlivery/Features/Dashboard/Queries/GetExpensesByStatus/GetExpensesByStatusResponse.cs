using Devlivery.Features.Expenses.Queries.GetAllExpenses;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public sealed record ExpenseStatusItem(ExpenseDisplayStatus Status, int Count, decimal Total);

public sealed record GetExpensesByStatusResponse(List<ExpenseStatusItem> Statuses);