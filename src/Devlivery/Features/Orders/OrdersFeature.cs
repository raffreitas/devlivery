using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Orders.Commands.DeleteOrder;
using Devlivery.Features.Orders.Commands.UpdateOrder;
using Devlivery.Features.Orders.Commands.UpdateOrderStatus;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Features.Orders.Queries.GetAllOrders;
using Devlivery.Features.Orders.Queries.GetOrderById;

namespace Devlivery.Features.Orders;

public static class OrdersFeature
{
    public static IServiceCollection AddOrderFeature(this IServiceCollection services)
    {
        // Register Repository
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Register Handlers
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<DeleteOrderHandler>();
        services.AddScoped<UpdateOrderStatusHandler>();
        services.AddScoped<GetAllOrdersHandler>();
        services.AddScoped<GetOrderByIdHandler>();
        services.AddScoped<UpdateOrderHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        CreateOrderEndpoint.MapEndpoint(group);
        DeleteOrderEndpoint.MapEndpoint(group);
        UpdateOrderStatusEndpoint.MapEndpoint(group);
        GetAllOrdersEndpoint.MapEndpoint(group);
        GetOrderByIdEndpoint.MapEndpoint(group);
        UpdateOrderEndpoint.MapEndpoint(group);

        return app;
    }
}