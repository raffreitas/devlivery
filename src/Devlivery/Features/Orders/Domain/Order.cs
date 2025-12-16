using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Features.Orders.Domain.ValueObjects;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain;

public sealed class Order : Entity
{
    public CustomerInfo Customer { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
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

    private Order()
    {
    }

    public Order(
        CustomerInfo customer,
        DeliveryAddress deliveryAddress,
        PaymentMethod paymentMethod,
        decimal deliveryFee,
        Guid establishmentId,
        List<OrderItem> items,
        string? notes = null
    )
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Pedido deve ter pelo menos um item", nameof(items));

        if (deliveryFee < 0)
            throw new ArgumentException("Taxa de entrega não pode ser negativa", nameof(deliveryFee));

        Customer = customer;
        DeliveryAddress = deliveryAddress;
        PaymentMethod = paymentMethod;
        Status = OrderStatus.Pending;
        DeliveryFee = deliveryFee;
        EstablishmentId = establishmentId;

        _items = items;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;

        CalculateTotal();
        AddDomainEvent(new OrderCreatedEvent(
            Id,
            EstablishmentId,
            Customer.Name,
            Total,
            PaymentMethod,
            CreatedAt));
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        // Validate state transitions
        if (Status == OrderStatus.Canceled)
        {
            throw new InvalidOperationException("Não é possível alterar o status de um pedido cancelado.");
        }

        if (Status == OrderStatus.Delivered && newStatus != OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Não é possível alterar o status de um pedido já entregue.");
        }

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

    public void UpdatePaymentMethod(PaymentMethod newPaymentMethod)
    {
        var oldPaymentMethod = PaymentMethod;
        PaymentMethod = newPaymentMethod;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderPaymentMethodChangedEvent(
            Id,
            EstablishmentId,
            oldPaymentMethod,
            PaymentMethod,
            Total,
            UpdatedAt
        ));
    }

    public void Delete()
    {
        AddDomainEvent(new OrderDeletedEvent(
            Id,
            EstablishmentId,
            Total,
            PaymentMethod,
            Status,
            DateTime.UtcNow
        ));
    }

    public void UpdateDetails(
        CustomerInfo customer,
        DeliveryAddress deliveryAddress,
        decimal deliveryFee,
        string? notes = null,
        List<OrderItem>? items = null)
    {
        var oldTotal = Total;
        
        Customer = customer;
        DeliveryAddress = deliveryAddress;
        DeliveryFee = deliveryFee;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;
        if (items is not null)
        {
            _items.Clear();
            _items.AddRange(items);
        }

        CalculateTotal();
        
        if (oldTotal != Total)
        {
            AddDomainEvent(new OrderUpdatedEvent(Id, EstablishmentId, oldTotal, Total, PaymentMethod, UpdatedAt));
        }
    }

    private void CalculateTotal()
    {
        Total = _items.Sum(i => i.TotalPrice) + DeliveryFee;
    }
}