namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public sealed record ExpenseCategoryItem(string Category, decimal Total, decimal Percentage);

public sealed record GetExpensesByCategoryResponse(List<ExpenseCategoryItem> Categories);

