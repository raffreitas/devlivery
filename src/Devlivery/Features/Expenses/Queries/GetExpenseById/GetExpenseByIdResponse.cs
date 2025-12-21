namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public sealed record GetExpenseByIdResponse(
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
    string CategoryName,
    bool IsActive,
    CategoryDto[] SubCategories
);