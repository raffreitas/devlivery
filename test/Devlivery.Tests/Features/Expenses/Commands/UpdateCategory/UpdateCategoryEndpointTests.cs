using System.Net;
using System.Text.Json;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateCategory;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class UpdateCategoryEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task UpdateCategory_WithValidRequest_ReturnsNoContent_And_Updates()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, establishment, accessToken) = await Prepare();

        var category = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithName("Categoria Original")
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var request = new { Name = "Categoria Atualizada", IsActive = false as bool? };

        // Act
        var response = await PutAsync($"/api/expenses/categories/{category.Id}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new { Name = "Nome", IsActive = true as bool? };

        var invalidId = Guid.NewGuid();

        // Act
        var response = await PutAsync($"/api/expenses/categories/{invalidId}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, establishment, accessToken) = await Prepare();

        var category = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            Name = new string('a', 500), // too long
            IsActive = true as bool?
        };

        // Act
        var response = await PutAsync($"/api/expenses/categories/{category.Id}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("success", out var success).ShouldBeTrue();
        success.GetBoolean().ShouldBeFalse();
        responseData.RootElement.TryGetProperty("errors", out var errors).ShouldBeTrue();
        errors.ValueKind.ShouldBe(JsonValueKind.Array);
        errors.GetArrayLength().ShouldBeGreaterThan(0);
    }
}