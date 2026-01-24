namespace Devlivery.Features.Orders.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Preparing,
    Ready,
    Delivered,
    Canceled
}