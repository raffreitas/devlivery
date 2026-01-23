using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Entities;

public sealed class OrderPayment : Entity
{
    public Guid EstablishmentId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public OrderPayment(Guid establishmentId, PaymentMethod paymentMethod, decimal amount)
    {
        EstablishmentId = establishmentId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        PaymentStatus = PaymentStatus.Pending;
    }

    public void Confirm()
    {
        if (PaymentStatus == PaymentStatus.Confirmed)
            throw new DomainException("Pagamento já está confirmado.");
        if (PaymentStatus == PaymentStatus.Cancelled)
            throw new DomainException("Não é possível confirmar um pagamento cancelado..");

        PaymentStatus = PaymentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (PaymentStatus == PaymentStatus.Cancelled) return;
        PaymentStatus = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(PaymentMethod paymentMethod, decimal amount)
    {
        if (PaymentStatus == PaymentStatus.Confirmed)
            throw new DomainException("Não é possível alterar um pagamento confirmado.");
        if (PaymentStatus == PaymentStatus.Cancelled)
            throw new DomainException("Não é possível alterar um pagamento cancelado.");

        PaymentMethod = paymentMethod;
        Amount = amount;
        UpdatedAt = DateTime.UtcNow;
    }
}