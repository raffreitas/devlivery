namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public sealed record GetOrdersByStatusResponse(
    int Pending,
    int Preparing,
    int Ready,
    int Delivered,
    int Canceled);