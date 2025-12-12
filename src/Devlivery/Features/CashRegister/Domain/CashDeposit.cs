using Devlivery.Shared.Domain;

namespace Devlivery.Features.CashRegister.Domain;

public sealed class CashDeposit : Entity
{
    public Guid CashSessionId { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public Guid AttendantId { get; private set; }
    public string AttendantName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime DepositedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CashDeposit()
    {
    }

    public CashDeposit(
        Guid cashSessionId,
        Guid establishmentId,
        Guid attendantId,
        string attendantName,
        decimal amount,
        string? notes)
    {
        CashSessionId = cashSessionId;
        EstablishmentId = establishmentId;
        AttendantId = attendantId;
        AttendantName = attendantName;
        Amount = amount;
        Notes = notes;
        DepositedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}