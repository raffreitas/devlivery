using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.CashRegister.Entities;

public sealed class CashSessionMovement : Entity
{
    public Guid EstablishmentId { get; private set; }
    public Guid CashSessionId { get; private set; }
    public CashSessionEntryType EntryType { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public Guid? RelatedOrderId { get; private set; }
    public Guid? OrderPaymentId { get; private set; }
    public string? Reason { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CashSessionMovement()
    {
    }

    public CashSessionMovement(
        Guid establishmentId,
        Guid cashSessionId,
        CashSessionEntryType entryType,
        decimal amount,
        Guid createdBy,
        PaymentMethod? paymentMethod = null,
        Guid? relatedOrderId = null,
        Guid? orderPaymentId = null,
        string? reason = null)
    {
        EstablishmentId = establishmentId;
        CashSessionId = cashSessionId;
        EntryType = entryType;
        Amount = amount >= 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
        PaymentMethod = paymentMethod;
        RelatedOrderId = relatedOrderId;
        OrderPaymentId = orderPaymentId;
        Reason = reason;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }
}