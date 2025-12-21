using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Features.Products.Commands.DeleteProduct;
using Devlivery.Features.Products.Commands.UpdateProduct;
using Devlivery.Features.Products.Domain;
using Devlivery.Features.Products.Infrastructure;
using Devlivery.Features.Products.Queries.GetAllProducts;
using Devlivery.Features.Products.Queries.GetProductById;

namespace Devlivery.Features.Products;

public static class ProductFeature
{
    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        // Register Repository
        services.AddScoped<IProductRepository, ProductRepository>();

        // Register Handlers
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<DeleteProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<GetAllProductsHandler>();
        services.AddScoped<GetProductByIdHandler>();
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