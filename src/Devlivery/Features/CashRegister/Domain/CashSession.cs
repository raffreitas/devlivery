using Devlivery.Shared.Domain;

namespace Devlivery.Features.CashRegister.Domain;

public sealed class CashSession : Entity
{
    public Guid EstablishmentId { get; private set; }
    public Guid AttendantId { get; private set; }
    public string AttendantName { get; private set; } = string.Empty;
    public decimal OpeningAmount { get; private set; }
    public decimal ExpectedCashAmount { get; private set; }
    public decimal? ClosingAmount { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }
    public CashSessionStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public int TotalOrders { get; private set; }
    public List<PaymentBreakdownItem> PaymentBreakdown { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private readonly List<CashDeposit> _deposits = [];
    public IReadOnlyCollection<CashDeposit> Deposits => _deposits.AsReadOnly();

    private CashSession()
    {
    }

    public CashSession(
        Guid establishmentId,
        Guid attendantId,
        string attendantName,
        decimal openingAmount,
        string? notes)
    {
        EstablishmentId = establishmentId;
        AttendantId = attendantId;
        AttendantName = attendantName;
        OpeningAmount = openingAmount;
        ExpectedCashAmount = openingAmount;
        Notes = notes;
        Status = CashSessionStatus.Open;
        StartAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTotals(decimal totalRevenue, int totalOrders, List<PaymentBreakdownItem> breakdown)
    {
        TotalRevenue = totalRevenue;
        TotalOrders = totalOrders;
        PaymentBreakdown = breakdown;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExpectedCashAmount(decimal expectedAmount)
    {
        ExpectedCashAmount = expectedAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDeposit(CashDeposit deposit)
    {
        _deposits.Add(deposit);
        var totalDeposits = Deposits.Sum(cd => cd.Amount);
        UpdateExpectedCashAmount(OpeningAmount + totalDeposits);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close(decimal closingAmount, string? notes)
    {
        if (Status == CashSessionStatus.Closed)
        {
            throw new InvalidOperationException("Este caixa já está fechado.");
        }

        ClosingAmount = closingAmount;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes;
        Status = CashSessionStatus.Closed;
        EndAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public sealed record PaymentBreakdownItem(string Method, decimal Amount, int Count);