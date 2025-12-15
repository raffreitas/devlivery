using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain;

public sealed class Order : Entity
{
    public string CustomerName { get; private set; }
    public string? CustomerPhone { get; private set; }
    public string DeliveryAddress { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(
        string customerName,
        string? customerPhone,
        string deliveryAddress,
        PaymentMethod paymentMethod,
        OrderStatus status,
        decimal deliveryFee,
        Guid establishmentId,
        string? notes = null
    )
    {
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        DeliveryAddress = deliveryAddress;
        PaymentMethod = paymentMethod;
        Status = status;
        DeliveryFee = deliveryFee;
        EstablishmentId = establishmentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;
    }

    /// <summary>
    /// Call this after the order is fully constructed and saved to raise the created event.
    /// </summary>
    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new OrderCreatedEvent(
            Id,
            EstablishmentId,
            CustomerName,
            Total,
            PaymentMethod,
            CreatedAt));
    }

    public void ReplaceItems(IEnumerable<OrderItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        UpdatedAt = DateTime.UtcNow;
        CalculateTotal();

        AddDomainEvent(new OrderUpdatedEvent(Id, EstablishmentId, Total, UpdatedAt));
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        CalculateTotal();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(
            Id,
            EstablishmentId,
            oldStatus,
            newStatus,
            PaymentMethod,
            Total,
            UpdatedAt
        ));
    }

    public void UpdateDetails(
        string customerName,
        string? customerPhone,
        string deliveryAddress,
        PaymentMethod paymentMethod,
        decimal deliveryFee,
        string? notes = null)
    {
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        DeliveryAddress = deliveryAddress;
        PaymentMethod = paymentMethod;
        DeliveryFee = deliveryFee;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;
        CalculateTotal();

        AddDomainEvent(new OrderUpdatedEvent(Id, EstablishmentId, Total, UpdatedAt));
    }

    private void CalculateTotal()
    {
        Total = _items.Sum(i => i.TotalPrice) + DeliveryFee;
    }
}