namespace Devlivery.WebApi.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("", GetAllProducts.Handle);
        group.MapGet("{id:guid}", GetProductById.Handle);
        group.MapPost("", CreateProduct.Handle);
        group.MapPut("{id:guid}", UpdateProduct.Handle);
        group.MapDelete("{id:guid}", DeleteProduct.Handle);

        return app;
    }
}
