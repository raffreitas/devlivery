using System.Net;
using System.Net.Http.Json;

using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Features.Products.Queries.GetAllProducts;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

/// <summary>Cross-cutting HTTP behavior shared by every authenticated endpoint.</summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class ApiContractTests(ApiFactory api)
{
    [Fact]
    public async Task GivenNoToken_WhenCallingProtectedEndpoint_ThenReturns401()
    {
        // Arrange
        using var client = api.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenInvalidCommand_WhenPosting_ThenReturns422WithCamelCaseFieldErrors()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var command = new CreateProductCommand("", "Descrição", 0m, "Pizzas", true);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/products", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problem = await ApiTenant.ReadProblemAsync(response);
        problem.Errors.ShouldNotBeNull().ShouldBe(new Dictionary<string, string>
        {
            ["name"] = "O campo 'Nome' é obrigatório.",
            ["price"] = "O campo 'Preço' deve ser maior que 0."
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task GivenProductOfAnotherTenant_WhenAccessingIt_ThenItIsInvisibleAndUntouchable()
    {
        // Arrange
        var owner = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithEstablishmentId(owner.EstablishmentId).Build();
        await owner.SeedAsync(product);
        var intruder = await api.CreateTenantAsync();
        var update = new { Name = "Invadido", Description = "x", Price = 1m, Category = "x", Available = true };

        // Act
        var list = await intruder.Client.GetAsync("/api/products");
        var get = await intruder.Client.GetAsync($"/api/products/{product.Id}");
        var put = await intruder.Client.PutAsJsonAsync($"/api/products/{product.Id}", update, ApiTenant.JsonOptions);
        var delete = await intruder.Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        (await ApiTenant.ReadDataAsync<List<GetAllProductsResponse>>(list)).ShouldBeEmpty();
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        put.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var stored = await owner.WithDbContextAsync(db => db.Products.FindAsync(product.Id).AsTask());
        stored.ShouldNotBeNull().Name.ShouldBe(product.Name);
    }
}