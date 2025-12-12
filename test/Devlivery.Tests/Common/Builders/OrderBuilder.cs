using Bogus;
using Devlivery.Features.Orders.Domain;

namespace Devlivery.Tests.Common.Builders;

public class OrderBuilder
{
    private readonly Faker _faker = new();

    private string _customerName;
    private string _customerPhone;
    private string _deliveryAddress;
    private PaymentMethod _paymentMethod;
    private decimal _deliveryFee;
    private OrderItem[] _orderItems;
    private Guid _establishmentId;
    private string? _notes;

    public OrderBuilder()
    {
        _customerName = _faker.Name.FirstName();
        _customerPhone = _faker.Phone.PhoneNumber("## #####-####");
        _deliveryAddress = _faker.Address.FullAddress();
        _paymentMethod = _faker.PickRandom<PaymentMethod>();
        _deliveryFee = _faker.Random.Decimal(0.0m, 20.0m);
        _orderItems = [];
        _notes = null;
    }

    public OrderBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    public OrderBuilder WithCustomerPhone(string customerPhone)
    {
        _customerPhone = customerPhone;
        return this;
    }

    public OrderBuilder WithDeliveryAddress(string deliveryAddress)
    {
        _deliveryAddress = deliveryAddress;
        return this;
    }

    public OrderBuilder WithPaymentMethod(PaymentMethod paymentMethod)
    {
        _paymentMethod = paymentMethod;
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
        if (_orderItems.Length == 0)
            throw new InvalidOperationException("No order items have been added");

        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        var order = new Order(
            customerName: _customerName,
            customerPhone: _customerPhone,
            deliveryAddress: _deliveryAddress,
            paymentMethod: _paymentMethod,
            status: OrderStatus.Pending,
            deliveryFee: _deliveryFee,
            establishmentId: _establishmentId,
            notes: _notes
        );

        foreach (var orderItem in _orderItems)
            order.AddItem(orderItem);

        return order;
    }
}