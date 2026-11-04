using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Entities;

public sealed class OrderItem : Entity, ITenantScoped
{
    public Guid ProductId { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    public OrderItem(Guid productId, Guid establishmentId, int quantity, decimal unitPrice, string? notes)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero");

        if (unitPrice < 0)
            throw new DomainException("Preço unitário não pode ser negativo");

        ProductId = productId;
        EstablishmentId = establishmentId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
    }
}