using System.Net;
using System.Net.Http.Json;

using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Features.Products.Queries.GetAllProducts;
using Devlivery.Features.Products.Queries.GetProductById;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class ProductsApiTests(ApiFactory api)
{
    [Fact]
    public async Task GivenTenantProducts_WhenListing_ThenReturnsThemNewestFirst()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var older = new ProductBuilder().WithName("Pizza Calabresa").WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(older);
        var newer = new ProductBuilder().WithName("Refrigerante").WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(newer);

        // Act
        var response = await tenant.Client.GetAsync("/api/products");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var products = await ApiTenant.ReadDataAsync<List<GetAllProductsResponse>>(response);
        products.Select(p => p.Name).ShouldBe(["Refrigerante", "Pizza Calabresa"]);
    }

    [Fact]
    public async Task GivenExistingProduct_WhenGettingById_ThenReturnsIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithName("Pizza Calabresa").WithPrice(49.90m)
            .WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(product);

        // Act
        var response = await tenant.Client.GetAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ApiTenant.ReadDataAsync<GetProductByIdResponse>(response);
        body.Name.ShouldBe("Pizza Calabresa");
        body.Price.ShouldBe(49.90m);
    }

    [Fact]
    public async Task GivenUnknownId_WhenGettingById_ThenReturns404ProblemDetails()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ApiTenant.ReadProblemAsync(response)).Detail.ShouldBe("Produto não encontrado");
    }

    [Fact]
    public async Task GivenValidProduct_WhenCreating_ThenReturns201WithLocationOfPersistedProduct()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var command = new CreateProductCommand("Pizza Calabresa", "Molho, calabresa e cebola", 49.90m, "Pizzas", true);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/products", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateProductResponse>(response);
        response.Headers.Location.ShouldBe(new Uri($"/api/products/{created.ProductId}", UriKind.Relative));
        var persisted = await tenant.WithDbContextAsync(db => db.Products.SingleAsync(p => p.Id == created.ProductId));
        persisted.Name.ShouldBe("Pizza Calabresa");
        persisted.EstablishmentId.ShouldBe(tenant.EstablishmentId);
    }

    [Fact]
    public async Task GivenExistingProduct_WhenUpdating_ThenReturns204AndPersistsChanges()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(product);
        var request = new { Name = "Pizza Portuguesa", Description = "Presunto e ovo", Price = 54.90m, Category = "Pizzas", Available = false };

        // Act
        var response = await tenant.Client.PutAsJsonAsync($"/api/products/{product.Id}", request, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await tenant.WithDbContextAsync(db => db.Products.SingleAsync(p => p.Id == product.Id));
        persisted.Name.ShouldBe("Pizza Portuguesa");
        persisted.Price.ShouldBe(54.90m);
        persisted.Available.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenProductUsedInOrder_WhenDeleting_ThenReturns422AndKeepsProduct()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        var order = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).Build())
            .Build();
        await tenant.SeedAsync(product, order);

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await tenant.WithDbContextAsync(db => db.Products.AnyAsync(p => p.Id == product.Id))).ShouldBeTrue();
    }

    [Fact]
    public async Task GivenUnusedProduct_WhenDeleting_ThenReturns204AndRemovesIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(product);

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await tenant.WithDbContextAsync(db => db.Products.AnyAsync(p => p.Id == product.Id))).ShouldBeFalse();
    }
}