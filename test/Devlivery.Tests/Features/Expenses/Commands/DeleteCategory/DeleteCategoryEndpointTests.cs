using System.Net;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteCategory;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class DeleteCategoryEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent_And_Removes()
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

        // Act
        var response = await DeleteAsync($"/api/expenses/categories/{category.Id}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var assertScope = Factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await assertDb.ExpenseCategories.FindAsync(category.Id);
        persisted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var invalidId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/expenses/categories/{invalidId}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}