using Bogus;

using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Orders.Domain.ValueObjects;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Tests.Common.Builders;

public class OrderBuilder
{
    private readonly Faker _faker = new();

    private string _customerName;
    private string? _customerPhone;
    private string _deliveryAddress;
    private string? _deliveryReference;
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
        _deliveryReference = null;
        _paymentMethod = _faker.PickRandom<PaymentMethod>();
        _deliveryFee = _faker.Random.Decimal(0.0m, 20.0m);
        _establishmentId = Guid.NewGuid();
        _orderItems = [];
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

        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            paymentMethod: _paymentMethod,
            deliveryFee: _deliveryFee,
            establishmentId: _establishmentId,
            items: items.ToList(),
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