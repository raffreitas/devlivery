using Devlivery.Features.Expenses.Queries.GetAllExpenses;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public sealed record GetUpcomingExpensesResponse(List<UpcomingExpenseItem> Expenses);

public sealed record UpcomingExpenseItem(
    Guid Id,
    CategoryDto Category,
    string? Supplier,
    string? Description,
    decimal Amount,
    DateOnly DueDate,
    ExpenseDisplayStatus Status);

