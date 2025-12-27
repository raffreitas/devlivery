using System.Net;
using System.Text.Json;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Queries.GetExpenseById;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetExpenseByIdEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetExpenseById_WithValidId_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, establishment, accessToken) = await Prepare();

        var category = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();

        var expense = new ExpenseBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithCategoryId(category.Id)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category);
        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync($"/api/expenses/{expense.Id}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var data = responseData.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().ShouldBe(expense.Id);
    }

    [Fact]
    public async Task GetExpenseById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var invalidId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/expenses/{invalidId}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

