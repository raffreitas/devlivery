using System.Net;
using System.Text.Json;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Queries.GetAllExpenseCategories;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetAllExpenseCategoriesEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetAllExpenseCategories_WithValidRequest_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, establishment, accessToken) = await Prepare();

        var category1 = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithName("Categoria 1")
            .Build();

        var category2 = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithName("Categoria 2")
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category1);
        dbContext.ExpenseCategories.Add(category2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync("/api/expenses/categories", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var data = responseData.RootElement.GetProperty("data");
        data.GetArrayLength().ShouldBeGreaterThanOrEqualTo(2);
    }
}