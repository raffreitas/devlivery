using System.Net;

using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.MarkExpenseAsPaid;

[Collection("Expenses Tests")]
[Trait("Category", "Integration Tests")]
public sealed class MarkExpenseAsPaidEndpointTests(ExpensesWebApplicationFactory factory)
    : WebApiBaseFixture<ExpensesWebApplicationFactory>(factory)
{
    [Fact]
    public async Task MarkExpenseAsPaid_WithValidData_ReturnsOk()
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
            .WithPaymentDate(null)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ExpenseCategories.Add(category);
        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            ExpenseId = expense.Id,
            PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        var response = await PatchAsync($"/api/expenses/{expense.Id}/mark-as-paid", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MarkExpenseAsPaid_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var request = new
        {
            ExpenseId = Guid.Empty,
            PaymentDate = default(DateOnly)
        };

        // Act
        var response = await PatchAsync("/api/expenses/00000000-0000-0000-0000-000000000000/mark-as-paid", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

