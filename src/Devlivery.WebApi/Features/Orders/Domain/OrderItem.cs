using Devlivery.WebApi.Shared.Domain;

namespace Devlivery.WebApi.Features.Orders.Domain;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    public OrderItem(Guid productId, int quantity, decimal unitPrice, string? notes)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
    }
}