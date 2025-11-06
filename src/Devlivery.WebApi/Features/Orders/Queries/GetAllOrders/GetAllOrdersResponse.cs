namespace Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersResponse(
    Guid Id,
    List<OrderItemDto> Items,
    string CustomerName,
    string CustomerPhone,
    string DeliveryAddress,
    string Status,
    decimal Total,
    decimal DeliveryFee,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record OrderItemDto(
    ProductDto Product,
    int Quantity,
    string? Notes);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available,
    DateTime CreatedAt,
    DateTime UpdatedAt);
