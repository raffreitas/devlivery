namespace Devlivery.Domain.Aggregates.Orders.Enums;

public enum OrderStatus
{
    Pending = 1,
    Preparing,
    Ready,
    Delivered,
    Canceled
}