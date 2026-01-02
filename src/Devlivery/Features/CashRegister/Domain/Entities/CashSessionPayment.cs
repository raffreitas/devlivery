using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.CashRegister.Domain.Entities;

public sealed class CashSessionPayment : Entity
{
    public Guid EstablishmentId { get; private set; }
    public Guid CashSessionId { get; private set; }
    public Guid OrderPaymentId { get; private set; }
    public CashSessionEntryType EntryType { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public bool IsReversal { get; private set; }
    public string? Reason { get; private set; }
    public Guid? RelatedOrderId { get; private set; }

    public CashSessionPayment(
        Guid establishmentId,
        Guid cashSessionId,
        Guid orderPaymentId,
        decimal amount,
        PaymentMethod paymentMethod)
    {
        EstablishmentId = establishmentId;
        CashSessionId = cashSessionId;
        OrderPaymentId = orderPaymentId;
        Amount = amount;
        EntryType = CashSessionEntryType.Payment;
        PaymentMethod = paymentMethod;
        RecordedAt = DateTime.UtcNow;
    }

    public CashSessionPayment(
        Guid establishmentId,
        Guid cashSessionId,
        Guid orderPaymentId,
        Guid relatedOrderId,
        decimal amount,
        PaymentMethod paymentMethod
    )
    {
        EstablishmentId = establishmentId;
        CashSessionId = cashSessionId;
        OrderPaymentId = orderPaymentId;
        Amount = amount;
        EntryType = CashSessionEntryType.Payment;
        PaymentMethod = paymentMethod;
        RelatedOrderId = relatedOrderId;
        RecordedAt = DateTime.UtcNow;
    }

    public CashSessionPayment(
        Guid establishmentId,
        Guid cashSessionId,
        Guid orderPaymentId,
        Guid relatedOrderId,
        decimal amount,
        PaymentMethod paymentMethod,
        bool isReversal,
        string? reason
    )
    {
        EstablishmentId = establishmentId;
        CashSessionId = cashSessionId;
        OrderPaymentId = orderPaymentId;
        Amount = isReversal ? -Math.Abs(amount) : amount;
        EntryType = isReversal ? CashSessionEntryType.Refund : CashSessionEntryType.Payment;
        PaymentMethod = paymentMethod;
        IsReversal = isReversal;
        Reason = reason;
        RelatedOrderId = relatedOrderId;
        RecordedAt = DateTime.UtcNow;
    }

    // Helper constructor to create explicit entry types (e.g. Change)
    public CashSessionPayment(
        Guid establishmentId,
        Guid cashSessionId,
        Guid? orderPaymentId,
        Guid? relatedOrderId,
        decimal amount,
        PaymentMethod paymentMethod,
        CashSessionEntryType entryType,
        string? reason = null)
    {
        EstablishmentId = establishmentId;
        CashSessionId = cashSessionId;
        OrderPaymentId = orderPaymentId ?? Guid.Empty;
        Amount = entryType == CashSessionEntryType.Refund || entryType == CashSessionEntryType.Change
            ? -Math.Abs(amount)
            : amount;
        EntryType = entryType;
        PaymentMethod = paymentMethod;
        IsReversal = entryType == CashSessionEntryType.Refund;
        Reason = reason;
        RelatedOrderId = relatedOrderId;
        RecordedAt = DateTime.UtcNow;
    }
}