namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

internal sealed record GetAllExpensesQueryDto(
    Guid Id,
    Guid EstablishmentId,
    string? Supplier,
    string? Description,
    decimal Amount,
    DateOnly DueDate,
    DateOnly? PaymentDate,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CategoryId,
    string CategoryName,
    bool CategoryIsActive,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    bool? ParentCategoryIsActive
);

public sealed record GetAllExpensesResponse(
    Guid Id,
    CategoryDto Category,
    string? Supplier,
    string? Description,
    decimal Amount,
    DateOnly DueDate,
    DateOnly? PaymentDate,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CategoryDto(
    Guid Id,
    string Name,
    bool IsActive,
    CategoryDto[] SubCategories
);