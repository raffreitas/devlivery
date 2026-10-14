using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Infrastructure.Persistence.Repositories;

namespace Devlivery.Features.Orders;

public static class OrdersFeature
{
    public static IServiceCollection AddOrderFeature(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}