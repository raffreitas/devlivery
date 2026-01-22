using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Features.Products.Commands.DeleteProduct;
using Devlivery.Features.Products.Commands.UpdateProduct;
using Devlivery.Features.Products.Queries.GetAllProducts;
using Devlivery.Features.Products.Queries.GetProductById;
using Devlivery.Infrastructure.Persistence.Repositories;

namespace Devlivery.Features.Products;

public static class ProductFeature
{
    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        // Register Repository
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        CreateProductEndpoint.MapEndpoint(group);
        DeleteProductEndpoint.MapEndpoint(group);
        UpdateProductEndpoint.MapEndpoint(group);
        GetAllProductsEndpoint.MapEndpoint(group);
        GetProductByIdEndpoint.MapEndpoint(group);

        return app;
    }
}