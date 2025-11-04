using Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

namespace Devlivery.WebApi.Features.Orders;

public static class OrdersFeature
{
    public static IServiceCollection AddOrderFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateOrderHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        CreateOrderEndpoint.MapEndpoint(group);
        group.MapGet("", GetAllOrders.Handle);
        group.MapGet("{id:guid}", GetOrderById.Handle);
        group.MapPatch("{id:guid}/status", UpdateOrderStatus.Handle);
        group.MapDelete("{id:guid}", DeleteOrder.Handle);

        return app;
    }
}