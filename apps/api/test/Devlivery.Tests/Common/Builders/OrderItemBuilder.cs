using Bogus;

using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Products;

namespace Devlivery.Tests.Common.Builders;

public sealed class OrderItemBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private Guid _productId;
    private Guid _establishmentId;
    private decimal _unitPrice;
    private int _quantity;
    private string? _notes;

    public OrderItemBuilder()
    {
        _productId = Guid.NewGuid();
        _establishmentId = Guid.NewGuid();
        _unitPrice = _faker.Random.Decimal(10, 200);
        _quantity = _faker.Random.Int(1, 10);
    }

    public OrderItemBuilder WithProduct(Product product)
    {
        _productId = product.Id;
        _unitPrice = product.Price;
        return this;
    }

    public OrderItemBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public OrderItemBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public OrderItemBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public OrderItem Build()
    {
        return new OrderItem(
            productId: _productId,
            establishmentId: _establishmentId,
            quantity: _quantity,
            unitPrice: _unitPrice,
            notes: _notes);
    }
}