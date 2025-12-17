using Bogus;

using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Products.Domain;

namespace Devlivery.Tests.Common.Builders;

public class OrderItemBuilder
{
    private readonly Faker _faker = new();
    private Product? _product;
    private Guid _productId;
    private decimal _unitPrice;
    private int _quantity;
    private string? _notes;
    private Guid _establishmentId;

    public OrderItemBuilder()
    {
        _productId = Guid.NewGuid();
        _unitPrice = _faker.Random.Decimal(10, 200);
        _quantity = _faker.Random.Int(1, 10);
        _notes = null;
        _establishmentId = Guid.NewGuid();
    }

    public OrderItemBuilder WithProduct(Product product)
    {
        _product = product;
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

    public OrderItemBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }


    public OrderItem Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        // Se um produto foi fornecido, usar os seus dados
        var productId = _product?.Id ?? _productId;
        var unitPrice = _product?.Price ?? _unitPrice;

        return new OrderItem(
            productId: productId,
            establishmentId: _establishmentId,
            quantity: _quantity,
            unitPrice: unitPrice,
            notes: _notes);
    }
}