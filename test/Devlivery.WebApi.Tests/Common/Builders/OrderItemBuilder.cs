using Bogus;
using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Features.Products.Domain;

namespace Devlivery.WebApi.Tests.Common.Builders;

public class OrderItemBuilder
{
    private readonly Faker _faker = new();
    private Product _product;
    private int _quantity;
    private string _notes;


    public OrderItemBuilder()
    {
        _quantity = _faker.Random.Int(min: 1);
        _notes = _faker.Lorem.Sentence();
        _product = new ProductBuilder().Build();
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


    public OrderItem Build()
    {
        return new OrderItem(
            productId: _product.Id,
            quantity: _quantity,
            unitPrice: _product.Price,
            notes: _notes);
    }
}