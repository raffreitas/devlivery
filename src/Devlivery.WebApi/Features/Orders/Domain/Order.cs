namespace Devlivery.WebApi.Features.Orders.Domain;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal DeliveryFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}