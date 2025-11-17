namespace Devlivery.WebApi.Features.Orders.Domain;

public enum OrderStatus
{
    Pending = 1,
    Preparing,
    Ready,
    Delivered,
    Canceled
}