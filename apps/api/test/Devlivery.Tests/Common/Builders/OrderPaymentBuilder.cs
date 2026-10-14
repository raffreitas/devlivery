using Bogus;

using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;

namespace Devlivery.Tests.Common.Builders;

public sealed class OrderPaymentBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private Guid _establishmentId;
    private PaymentMethod _paymentMethod;
    private decimal _amount;
    private PaymentStatus _status;

    public OrderPaymentBuilder()
    {
        _establishmentId = Guid.NewGuid();
        _paymentMethod = _faker.PickRandom<PaymentMethod>();
        _amount = _faker.Random.Decimal(10m, 500m);
        _status = PaymentStatus.Pending;
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

    public OrderPaymentBuilder AsConfirmed()
    {
        _status = PaymentStatus.Confirmed;
        return this;
    }

    public OrderPaymentBuilder AsCancelled()
    {
        _status = PaymentStatus.Cancelled;
        return this;
    }

    public OrderPayment Build()
    {
        var payment = new OrderPayment(_establishmentId, _paymentMethod, _amount);

        if (_status == PaymentStatus.Confirmed)
            payment.Confirm();
        else if (_status == PaymentStatus.Cancelled)
            payment.Cancel();

        return payment;
    }
}