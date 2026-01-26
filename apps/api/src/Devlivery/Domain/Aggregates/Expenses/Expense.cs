using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Expenses;

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
            throw new DomainException("O valor da despesa deve ser maior que zero.");

        CategoryId = categoryId;
        Supplier = supplier;
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        PaymentDate = paymentDate;
        EstablishmentId = establishmentId;
        Status = paymentDate is not null ? ExpenseStatus.Paid : ExpenseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue(DateOnly referenceDate)
    {
        if (Status == ExpenseStatus.Paid) return false;
        return Status == ExpenseStatus.Pending && DueDate < referenceDate;
    }

    public bool IsDueToday(DateOnly referenceDate)
    {
        if (Status == ExpenseStatus.Paid) return false;
        return Status == ExpenseStatus.Pending && DueDate == referenceDate;
    }

    public void Update(
        Guid? categoryId = null,
        decimal? amount = null,
        DateOnly? dueDate = null,
        string? supplier = null,
        string? description = null)
    {
        if (Status != ExpenseStatus.Pending)
        {
            throw new DomainException(
                "Não é permitido alterar uma despesa Paga ou Cancelada. Estorne o pagamento primeiro.");
        }

        Description = description ?? Description;
        Amount = amount ?? Amount;
        DueDate = dueDate ?? DueDate;
        CategoryId = categoryId ?? CategoryId;
        Supplier = supplier ?? Supplier;

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateOnly paymentDate)
    {
        if (Status == ExpenseStatus.Paid) return;

        if (Status == ExpenseStatus.Cancelled)
            throw new DomainException("Não é possível pagar uma despesa cancelada.");

        Status = ExpenseStatus.Paid;
        PaymentDate = paymentDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ExpenseStatus.Paid)
            throw new DomainException(
                "Não é possível cancelar uma despesa já paga. Faça o estorno primeiro."
            );

        Status = ExpenseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnmarkAsPaid()
    {
        if (Status != ExpenseStatus.Paid) return;

        Status = ExpenseStatus.Pending;
        PaymentDate = null;
        UpdatedAt = DateTime.UtcNow;
    }
}