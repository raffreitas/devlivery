namespace Devlivery.WebApi.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("", GetAllOrders.Handle);
        group.MapGet("{id:guid}", GetOrderById.Handle);
        group.MapPost("", CreateOrder.Handle);
        group.MapPatch("{id:guid}/status", UpdateOrderStatus.Handle);
        group.MapDelete("{id:guid}", DeleteOrder.Handle);

        return app;
    }
}
