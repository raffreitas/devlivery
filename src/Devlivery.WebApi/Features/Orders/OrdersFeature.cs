using Devlivery.WebApi.Features.Orders.Commands.CreateOrder;
using Devlivery.WebApi.Features.Orders.Commands.DeleteOrder;
using Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;
using Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;
using Devlivery.WebApi.Features.Orders.Queries.GetOrderById;
using Devlivery.WebApi.Features.Orders.Commands.UpdateOrder;

namespace Devlivery.WebApi.Features.Orders;

public static class OrdersFeature
{
    public static IServiceCollection AddOrderFeature(this IServiceCollection services)
    {
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