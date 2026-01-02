using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.CashRegister.Domain;

public sealed class CashSession : Entity
{
    private readonly List<CashSessionMovement> _movements = [];

    public Guid EstablishmentId { get; private set; }
    public Guid AttendantId { get; private set; }
    public string AttendantName { get; private set; } = null!;
    public decimal OpeningAmount { get; private set; }
    public decimal? ClosingAmount { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }
    public CashSessionStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<CashSessionMovement> Movements => _movements.AsReadOnly();
    public decimal TotalRevenue => _movements.Where(m => m.EntryType == CashSessionEntryType.Payment).Sum(m => m.Amount)
        - _movements.Where(m => m.EntryType == CashSessionEntryType.Refund).Sum(m => m.Amount);
    public int TotalOrders => _movements.Where(m => m.OrderPaymentId != null).Select(x => x.OrderPaymentId).Distinct().Count();
    public decimal ExpectedCashAmount => OpeningAmount + TotalDeposits() + TotalCashPayments();

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
        Notes = notes;
        Status = CashSessionStatus.Open;
        StartAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPayment(Guid orderPaymentId, decimal amount, PaymentMethod paymentMethod, Guid relatedOrderId)
    {
        if (Status != CashSessionStatus.Open)
            throw new InvalidOperationException("Não é possível adicionar pagamentos a um caixa fechado.");

        if (_movements.Exists(m => m.OrderPaymentId == orderPaymentId))
            return;

        _movements.Add(new CashSessionMovement(
            establishmentId: EstablishmentId,
            cashSessionId: Id,
            entryType: CashSessionEntryType.Payment,
            amount: amount >= 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount)),
            createdBy: AttendantId,
            paymentMethod: paymentMethod,
            relatedOrderId: relatedOrderId,
            orderPaymentId: orderPaymentId));

        UpdatedAt = DateTime.UtcNow;
    }

    public void AddReversal(Guid originalOrderPaymentId, decimal amount, PaymentMethod paymentMethod, string reason, Guid relatedOrderId)
    {
        if (Status != CashSessionStatus.Open)
            throw new InvalidOperationException("Não é possível adicionar reversões a um caixa fechado.");

        if (HasReversalFor(originalOrderPaymentId))
            return;

        _movements.Add(new CashSessionMovement(
            establishmentId: EstablishmentId,
            cashSessionId: Id,
            entryType: CashSessionEntryType.Refund,
            amount: amount >= 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount)),
            createdBy: AttendantId,
            paymentMethod: paymentMethod,
            relatedOrderId: relatedOrderId,
            orderPaymentId: originalOrderPaymentId,
            reason: reason));

        UpdatedAt = DateTime.UtcNow;
    }

    public void AddChange(Guid relatedOrderId, decimal changeAmount, PaymentMethod paymentMethod = PaymentMethod.Cash)
    {
        if (Status != CashSessionStatus.Open)
            throw new InvalidOperationException("Não é possível adicionar troco a um caixa fechado.");

        if (changeAmount <= 0)
            return;

        if (HasChangeFor(relatedOrderId))
            return;

        _movements.Add(new CashSessionMovement(
            establishmentId: EstablishmentId,
            cashSessionId: Id,
            entryType: CashSessionEntryType.Change,
            amount: changeAmount >= 0 ? changeAmount : throw new ArgumentOutOfRangeException(nameof(changeAmount)),
            createdBy: AttendantId,
            paymentMethod: paymentMethod,
            relatedOrderId: relatedOrderId,
            orderPaymentId: null));

        UpdatedAt = DateTime.UtcNow;
    }

    public CashSessionMovement AddDeposit(decimal amount, Guid createdBy, string? reason)
    {
        var movement = new CashSessionMovement(
            establishmentId: EstablishmentId,
            cashSessionId: Id,
            entryType: CashSessionEntryType.Deposit,
            amount: amount >= 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount)),
            createdBy: createdBy,
            paymentMethod: null,
            relatedOrderId: null,
            orderPaymentId: null,
            reason: reason);

        _movements.Add(movement);
        UpdatedAt = DateTime.UtcNow;
        return movement;
    }

    public bool HasReversalFor(Guid originalOrderPaymentId)
        => _movements.Any(m => m.EntryType == CashSessionEntryType.Refund && m.OrderPaymentId == originalOrderPaymentId);

    public bool HasChangeFor(Guid orderId)
        => _movements.Any(p => p.EntryType == CashSessionEntryType.Change && p.RelatedOrderId == orderId);

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

    public decimal TotalDeposits() => Movements.Where(m => m.EntryType == CashSessionEntryType.Deposit).Sum(m => m.Amount);

    public decimal TotalCashPayments()
    {
        var payments = Movements.Where(m => m.PaymentMethod == PaymentMethod.Cash && m.EntryType == CashSessionEntryType.Payment).Sum(m => m.Amount);
        var refunds = Movements.Where(m => m.PaymentMethod == PaymentMethod.Cash && m.EntryType == CashSessionEntryType.Refund).Sum(m => m.Amount);
        var change = Movements.Where(m => m.EntryType == CashSessionEntryType.Change).Sum(m => m.Amount);
        return payments - refunds - change;
    }
}