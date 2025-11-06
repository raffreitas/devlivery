namespace Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderResponse(
    Guid Id,
    OrderItemResponseDto[] Items,
    string CustomerName,
    string CustomerPhone,
    string DeliveryAddress,
    string Status,
    decimal Total,
    decimal DeliveryFee,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record OrderItemResponseDto(
    ProductResponseDto Product,
    int Quantity,
    string? Notes
);

public sealed record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    DateTime CreatedAt,
    DateTime UpdatedAt
);