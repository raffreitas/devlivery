using Devlivery.WebApi.Features.Products.Domain;

namespace Devlivery.WebApi.Features.Orders.Domain;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
