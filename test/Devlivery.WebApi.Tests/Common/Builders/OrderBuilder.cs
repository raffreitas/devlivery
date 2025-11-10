using Bogus;
using Devlivery.WebApi.Features.Orders.Domain;

namespace Devlivery.WebApi.Tests.Common.Builders;

public class OrderBuilder
{
    private readonly Faker _faker = new();
    private Guid _orderId = Guid.CreateVersion7();
    private string _customerName;
    private string _customerPhone;
    private string _deliveryAddress;
    private PaymentMethod _paymentMethod;
    private decimal _total;
    private decimal _deliveryFee;
    private OrderItem[] _orderItems;

    public OrderBuilder()
    {
        _customerName = _faker.Name.FirstName();
        _customerPhone = _faker.Phone.PhoneNumber();
        _deliveryAddress = _faker.Address.FullAddress();
        _paymentMethod = _faker.PickRandom<PaymentMethod>();
        _total = _faker.Random.Decimal(10.0m, 500.0m);
        _deliveryFee = _faker.Random.Decimal(0.0m, 20.0m);
        _orderItems = [];
    }

    public OrderBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
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

    // Remove WithTotal and calculate total from order items + delivery fee
    public OrderBuilder WithTotal(decimal total)
    {
        _total = total;
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


    public Order Build()
    {
        if (_orderItems.Length == 0)
            throw new InvalidOperationException("No order items have been added");

        return new Order
        {
            Id = _orderId,
            CustomerName = _customerName,
            CustomerPhone = _customerPhone,
            DeliveryAddress = _deliveryAddress,
            Status = "pending",
            PaymentMethod = _paymentMethod,
            Total = _total,
            DeliveryFee = _deliveryFee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}