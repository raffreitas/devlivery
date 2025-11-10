using Bogus;
using Devlivery.WebApi.Features.Orders.Domain;

namespace Devlivery.WebApi.Tests.Common.Builders;

public class OrderItemBuilder
{
    private readonly Faker _faker = new();
    private Guid _orderItemId = Guid.NewGuid();
    private Guid _productId = Guid.NewGuid();
    private Guid _orderId = Guid.NewGuid();
    private int _quantity;
    private string _notes;


    public OrderItemBuilder()
    {
        _quantity = _faker.Random.Int(min: 1);
        _notes = _faker.Lorem.Sentence();
    }

    public OrderItemBuilder WithOrderItemId(Guid orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    public OrderItemBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public OrderItemBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }


    public OrderItem Build()
    {
        return new OrderItem
        {
            Id = _orderItemId,
            ProductId = _productId,
            OrderId = _orderId,
            Quantity = _quantity,
            Notes = _notes
        };
    }
}