using System.Net;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteExpense;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class DeleteExpenseEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task DeleteExpense_WithValidId_ReturnsOk()
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
        var response = await DeleteAsync($"/api/expenses/{expense.Id}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteExpense_WithInvalidId_ReturnsUnprocessableEntity()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        // Act
        var response = await DeleteAsync("/api/expenses/00000000-0000-0000-0000-000000000000", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }
}