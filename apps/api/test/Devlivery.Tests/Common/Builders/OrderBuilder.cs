using Bogus;

using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.Common.ValueObjects;

namespace Devlivery.Tests.Common.Builders;

public class OrderBuilder
{
    private readonly Faker _faker = new();

    private string _customerName;
    private string? _customerPhone;
    private string _deliveryAddress;
    private string? _deliveryReference;
    private decimal _deliveryFee;
    private OrderItem[] _orderItems;
    private readonly List<OrderPayment> _payments = [];
    private Guid _establishmentId;
    private string? _notes;

    public OrderBuilder()
    {
        _customerName = _faker.Name.FullName();
        _customerPhone = _faker.Phone.PhoneNumber("## #####-####");
        _deliveryAddress = _faker.Address.FullAddress();
        _deliveryReference = null;
        _deliveryFee = _faker.Random.Decimal(0.0m, 20.0m);
        _establishmentId = Guid.NewGuid();
        _orderItems = [];
        _payments = [];
        _notes = null;
    }

    public OrderBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    public OrderBuilder WithCustomerPhone(string? customerPhone)
    {
        _customerPhone = customerPhone;
        return this;
    }

    public OrderBuilder WithDeliveryAddress(string deliveryAddress)
    {
        _deliveryAddress = deliveryAddress;
        return this;
    }

    public OrderBuilder WithDeliveryReference(string? deliveryReference)
    {
        _deliveryReference = deliveryReference;
        return this;
    }

    public OrderBuilder WithPaymentMethod(PaymentMethod paymentMethod, decimal? amount = null)
    {
        _payments.Add(new OrderPayment(_establishmentId, paymentMethod, amount ?? 0));
        return this;
    }

    public OrderBuilder WithPayment(OrderPayment payment)
    {
        _payments.Add(payment);
        return this;
    }

    public OrderBuilder WithCustomPayments(IEnumerable<OrderPayment> payments)
    {
        _payments.Clear();
        _payments.AddRange(payments);
        return this;
    }

    public OrderBuilder WithDeliveryFee(decimal deliveryFee)
    {
        _deliveryFee = deliveryFee;
        return this;
    }

    public OrderBuilder WithItems(params OrderItem[] items)
    {
        _orderItems = items;
        return this;
    }

    public OrderBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public OrderBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public Order Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        // Se não houver items, criar um item padrão
        var items = _orderItems.Length == 0
            ? new[] { CreateDefaultOrderItem() }
            : _orderItems;

        // Create value objects
        PhoneNumber? phone = null;
        if (!string.IsNullOrEmpty(_customerPhone))
        {
            phone = new PhoneNumber(_customerPhone);
        }

        var customer = CustomerInfo.Create(_customerName, phone);
        var deliveryAddress = new DeliveryAddress(_deliveryAddress, _deliveryReference);

        var totalItems = items.Sum(i => i.TotalPrice) + _deliveryFee;

        // Se não houver pagamentos, criar um pagamento total padrão
        if (_payments.Count == 0)
        {
            _payments.Add(new OrderPayment(_establishmentId, _faker.PickRandom<PaymentMethod>(), totalItems));
        }
        else
        {
            // Ajustar o valor do primeiro pagamento se ele for 0 (caso tenha sido adicionado sem valor)
            if (_payments.Count == 1 && _payments[0].Amount == 0)
            {
                var p = _payments[0];
                _payments.Clear();
                _payments.Add(new OrderPayment(_establishmentId, p.PaymentMethod, totalItems));
            }
        }

        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: _deliveryFee,
            establishmentId: _establishmentId,
            items: items.ToList(),
            payments: _payments,
            notes: _notes
        );

        return order;
    }

    private OrderItem CreateDefaultOrderItem()
    {
        return new OrderItemBuilder()
            .WithEstablishmentId(_establishmentId)
            .WithQuantity(_faker.Random.Int(1, 5))
            .Build();
    }
}