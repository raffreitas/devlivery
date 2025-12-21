using Devlivery.Features.Expenses.Domain.Enums;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Expenses.Domain.Aggregates.Expenses;

public sealed class Expense : Entity
{
    public Guid CategoryId { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public string? Supplier { get; private set; }
    public string? Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public ExpenseStatus CurrentStatus => CalculateCurrentStatus();

    public Expense(
        Guid establishmentId,
        Guid categoryId,
        decimal amount,
        DateOnly dueDate,
        string? supplier = null,
        string? description = null,
        DateOnly? paymentDate = null
    )
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        CategoryId = categoryId;
        Supplier = supplier;
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        PaymentDate = paymentDate;
        EstablishmentId = establishmentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Status = CalculateCurrentStatus();
    }

    public void Update(
        Guid? categoryId,
        decimal? amount = null,
        DateOnly? dueDate = null,
        string? supplier = null,
        string? description = null)
    {
        CategoryId = categoryId ?? CategoryId;
        Amount = amount ?? Amount;
        DueDate = dueDate ?? DueDate;
        Supplier = supplier ?? Supplier;
        Description = description ?? Description;
        UpdatedAt = DateTime.UtcNow;

        Status = CalculateCurrentStatus();
    }

    public void MarkAsPaid(DateOnly paymentDate)
    {
        PaymentDate = paymentDate;
        Status = ExpenseStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus()
    {
        Status = CalculateCurrentStatus();
        UpdatedAt = DateTime.UtcNow;
    }

    private ExpenseStatus CalculateCurrentStatus()
    {
        if (PaymentDate.HasValue)
            return ExpenseStatus.Paid;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var due = DueDate;

        if (due < today)
            return ExpenseStatus.Overdue;

        return due <= today.AddDays(3)
            ? ExpenseStatus.Scheduled
            : ExpenseStatus.Pending;
    }
}