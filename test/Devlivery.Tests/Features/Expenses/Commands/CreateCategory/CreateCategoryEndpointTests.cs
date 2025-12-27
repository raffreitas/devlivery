using System.Net;

using Devlivery.Tests.Common;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateCategory;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateCategoryEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreated_And_Persists()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new { Name = "Nova Categoria" };

        // Act
        var response = await PostAsync("/api/expenses/categories", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateCategory_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new
        {
            Name = "" // invalid
        };

        // Act
        var response = await PostAsync("/api/expenses/categories", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}