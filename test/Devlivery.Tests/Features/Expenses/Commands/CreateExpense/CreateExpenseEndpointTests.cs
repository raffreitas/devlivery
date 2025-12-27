using System.Net;
using System.Text.Json;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateExpense;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateExpenseEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateExpense_WithValidData_ReturnsCreatedAndExpense()
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
            CategoryId = category.Id,
            Amount = Faker.Random.Decimal(10, 1000),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier = Faker.Company.CompanyName(),
            Description = Faker.Lorem.Sentence(),
            PaymentDate = (DateOnly?)null
        };

        // Act
        var response = await PostAsync("/api/expenses", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var data = responseData.RootElement.GetProperty("data");
        data.GetProperty("expenseId").GetGuid().ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateExpense_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new
        {
            CategoryId = Guid.Empty,
            Amount = -10.00m,
            DueDate = default(DateOnly),
            Supplier = "",
            Description = "",
            PaymentDate = (DateOnly?)null
        };

        // Act
        var response = await PostAsync("/api/expenses", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateExpense_WithInactiveCategory_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, establishment, accessToken) = await Prepare();

        var category = new CategoryBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithIsActive(false)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            CategoryId = category.Id,
            Amount = 100.00m,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier = (string?)null,
            Description = (string?)null,
            PaymentDate = (DateOnly?)null
        };

        // Act
        var response = await PostAsync("/api/expenses", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

