using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Entities;

/// <summary>
/// Child Entity of Order aggregate.
/// OrderItem HAS identity within the Order context, but cannot exist independently.
/// It is NOT a Value Object because it needs to be tracked and updated individually.
/// 
/// DDD Pattern: Child Entity
/// - Has its own Id for tracking within the aggregate
/// - Only accessible through Order (aggregate root)
/// - No repository of its own
/// - Lifecycle controlled by Order
/// </summary>
public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    // EF Core constructor
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

    /// <summary>
    /// Updates the quantity of this item.
    /// Business logic: quantity must be positive.
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero");

        Quantity = newQuantity;
    }

    /// <summary>
    /// Updates the notes for this item.
    /// </summary>
    public void UpdateNotes(string? newNotes)
    {
        Notes = newNotes;
    }

    /// <summary>
    /// Updates the unit price (e.g., when product price changes).
    /// </summary>
    internal void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new DomainException("Preço unitário não pode ser negativo");

        UnitPrice = newUnitPrice;
    }
}