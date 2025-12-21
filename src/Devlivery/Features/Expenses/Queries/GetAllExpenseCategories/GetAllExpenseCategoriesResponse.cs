namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public sealed record GetAllExpenseCategoriesResponse(
    Guid Id,
    string Name,
    bool IsActive,
    GetAllExpenseCategoriesResponse[] SubCategories
);