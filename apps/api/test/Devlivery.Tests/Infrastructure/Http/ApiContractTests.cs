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
    [Theory]
    [InlineData("/api/products")]
    [InlineData("/api/v2/products")]
    [InlineData("/api/v1/version")]
    [InlineData("/api/v2/version")]
    [InlineData("/api/v3/version")]
    public async Task GivenAuthenticatedClient_WhenCallingUnmappedVersion_ThenReturns404(string path)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync(path);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenV1Endpoint_WhenCallingIt_ThenReportsSupportedVersion()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues("api-supported-versions").ShouldBe(new[] { "1.0" });
    }

    [Theory]
    [InlineData("/api/v1/products")]
    public async Task GivenNoToken_WhenCallingProtectedEndpoint_ThenReturns401(string path)
    {
        // Arrange
        using var client = api.CreateClient();

        // Act
        var response = await client.GetAsync(path);

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
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/products", command, ApiTenant.JsonOptions);

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
        var list = await intruder.Client.GetAsync("/api/v1/products");
        var get = await intruder.Client.GetAsync($"/api/v1/products/{product.Id}");
        var put = await intruder.Client.PutAsJsonAsync($"/api/v1/products/{product.Id}", update, ApiTenant.JsonOptions);
        var delete = await intruder.Client.DeleteAsync($"/api/v1/products/{product.Id}");

        // Assert
        (await ApiTenant.ReadDataAsync<List<GetAllProductsResponse>>(list)).ShouldBeEmpty();
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        put.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var stored = await owner.WithDbContextAsync(db => db.Products.FindAsync(product.Id).AsTask());
        stored.ShouldNotBeNull().Name.ShouldBe(product.Name);
    }
}
