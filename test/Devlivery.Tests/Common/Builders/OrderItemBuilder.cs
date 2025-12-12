using Bogus;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Domain;

namespace Devlivery.Tests.Common.Builders;

public class OrderItemBuilder
{
    private readonly Faker _faker = new();
    private Product? _product;
    private int _quantity;
    private string _notes;
    private Guid _establishmentId;

    public OrderItemBuilder()
    {
        _quantity = _faker.Random.Int(min: 1);
        _notes = _faker.Lorem.Sentence();
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

        if (_product == null)
            throw new InvalidOperationException("No product has been added");

        return new OrderItem(
            productId: _product.Id,
            establishmentId: _establishmentId,
            quantity: _quantity,
            unitPrice: _product.Price,
            notes: _notes);
    }
}