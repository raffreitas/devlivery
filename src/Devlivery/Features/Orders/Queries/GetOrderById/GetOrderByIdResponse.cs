namespace Devlivery.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdResponse(
    Guid Id,
    List<OrderItemDto> Items,
    string CustomerName,
    string? CustomerPhone,
    string DeliveryAddress,
    string? Notes,
    string Status,
    decimal Total,
    decimal DeliveryFee,
    OrderPaymentDto[] Payments,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record OrderPaymentDto(
    Guid Id,
    decimal Amount,
    string PaymentMethod);

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