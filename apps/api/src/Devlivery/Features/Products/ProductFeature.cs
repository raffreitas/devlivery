using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Infrastructure.Persistence.Repositories;

namespace Devlivery.Features.Products;

public static class ProductFeature
{
    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}