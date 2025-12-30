using System.Net;
using System.Text.Json;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateExpense;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class UpdateExpenseEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task UpdateExpense_WithValidData_ReturnsNoContent()
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

        var request = new
        {
            ExpenseId = expense.Id,
            CategoryId = (Guid?)null,
            Amount = 250.00m,
            DueDate = (DateOnly?)null,
            Supplier = "Fornecedor Atualizado",
            Description = "Descrição Atualizada"
        };

        // Act
        var response = await PutAsync($"/api/expenses/{expense.Id}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateExpense_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new
        {
            ExpenseId = Guid.Empty,
            CategoryId = (Guid?)null,
            Amount = -10.00m,
            DueDate = (DateOnly?)null,
            Supplier = "",
            Description = ""
        };

        // Act
        var response = await PutAsync("/api/expenses/00000000-0000-0000-0000-000000000000", request, accessToken);

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