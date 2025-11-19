using Devlivery.WebApi.Shared.Domain;

namespace Devlivery.WebApi.Features.Orders.Domain;

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

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(
        string customerName,
        string? customerPhone,
        string deliveryAddress,
        PaymentMethod paymentMethod,
        OrderStatus status,
        decimal deliveryFee,
        Guid establishmentId
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
    }

    public void ClearItems()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
        CalculateTotal();
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        CalculateTotal();
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string customerName,
        string? customerPhone,
        string deliveryAddress,
        PaymentMethod paymentMethod,
        decimal deliveryFee)
    {
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        DeliveryAddress = deliveryAddress;
        PaymentMethod = paymentMethod;
        DeliveryFee = deliveryFee;
        UpdatedAt = DateTime.UtcNow;
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        Total = _items.Sum(i => i.TotalPrice) + DeliveryFee;
    }
}