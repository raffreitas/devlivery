using Bogus;

using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Domain.Enums;

namespace Devlivery.Tests.Common.Builders;

public class OrderPaymentBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private Guid _establishmentId;
    private PaymentMethod _paymentMethod;
    private decimal _amount;
    private PaymentStatus _paymentStatus;
    private DateTime? _confirmedAt;

    public OrderPaymentBuilder()
    {
        _establishmentId = Guid.NewGuid();
        _paymentMethod = _faker.PickRandom<PaymentMethod>();
        _amount = _faker.Random.Decimal(10m, 500m);
        _paymentStatus = PaymentStatus.Pending;
        _confirmedAt = null;
    }

    public OrderPaymentBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public OrderPaymentBuilder WithPaymentMethod(PaymentMethod paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public OrderPaymentBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public OrderPaymentBuilder WithStatus(PaymentStatus status)
    {
        _paymentStatus = status;
        if (status == PaymentStatus.Confirmed)
        {
            _confirmedAt = DateTime.UtcNow;
        }
        return this;
    }

    public OrderPaymentBuilder AsConfirmed()
    {
        _paymentStatus = PaymentStatus.Confirmed;
        _confirmedAt = DateTime.UtcNow;
        return this;
    }

    public OrderPaymentBuilder AsCancelled()
    {
        _paymentStatus = PaymentStatus.Cancelled;
        _confirmedAt = null;
        return this;
    }

    public OrderPaymentBuilder AsPending()
    {
        _paymentStatus = PaymentStatus.Pending;
        _confirmedAt = null;
        return this;
    }

    public OrderPayment Build()
    {
        var payment = new OrderPayment(_establishmentId, _paymentMethod, _amount);

        // Use reflection to set internal state for testing purposes
        if (_paymentStatus == PaymentStatus.Confirmed && _confirmedAt.HasValue)
        {
            payment.Confirm();
        }
        else if (_paymentStatus == PaymentStatus.Cancelled)
        {
            payment.Cancel();
        }

        return payment;
    }
}
