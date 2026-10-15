using System.Net;
using System.Net.Http.Json;

using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Commands.CreateCategory;
using Devlivery.Features.Expenses.Commands.CreateExpense;
using Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Features.Expenses.Queries.GetExpenseById;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class ExpensesApiTests(ApiFactory api)
{
    private static readonly DateOnly DueDate = new(2026, 5, 10);

    [Fact]
    public async Task GivenQueryStringFilters_WhenListing_ThenBindsCategoryStatusAndDueDateRange()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (parent, subcategory) = await SeedCategoriesAsync(tenant);
        var otherCategory = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(otherCategory);
        await SeedExpenseAsync(tenant, subcategory, "Paga em maio", e => e.WithDueDate(DueDate).AsPaid(DueDate));
        await SeedExpenseAsync(tenant, subcategory, "Paga em junho", e => e.WithDueDate(DueDate.AddMonths(1)).AsPaid(DueDate));
        await SeedExpenseAsync(tenant, otherCategory, "Outra categoria", e => e.WithDueDate(DueDate).AsPaid(DueDate));

        // Act
        var response = await tenant.Client.GetAsync(
            $"/api/v1/expenses?categoryId={parent.Id}&status=Paid&start=2026-05-01&end=2026-05-31");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var expenses = await ApiTenant.ReadDataAsync<List<GetAllExpensesResponse>>(response);
        var expense = expenses.ShouldHaveSingleItem();
        expense.Description.ShouldBe("Paga em maio");
        expense.Status.ShouldBe(ExpenseDisplayStatus.Paid);
    }

    [Fact]
    public async Task GivenExistingExpense_WhenGettingById_ThenReturnsIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var expense = await SeedExpenseAsync(tenant, subcategory, "Farinha", e => e.WithAmount(150m));

        // Act
        var response = await tenant.Client.GetAsync($"/api/v1/expenses/{expense.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ApiTenant.ReadDataAsync<GetExpenseByIdResponse>(response);
        body.Description.ShouldBe("Farinha");
        body.Amount.ShouldBe(150m);
        body.Category.Id.ShouldBe(subcategory.Id);
    }

    [Fact]
    public async Task GivenUnknownId_WhenGettingExpenseById_ThenReturns404()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync($"/api/v1/expenses/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenActiveAndInactiveCategories_WhenListingCategories_ThenReturnsActiveTreeSortedByName()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var operational = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Operacional").Build();
        var supplies = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Insumos").WithParent(operational).Build();
        var cleaning = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Limpeza").WithParent(operational).Build();
        var retired = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Antiga").WithParent(operational).AsInactive().Build();
        var administrative = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Administrativo").Build();
        var inactiveParent = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Desativada").AsInactive().Build();
        await tenant.SeedAsync(operational, supplies, cleaning, retired, administrative, inactiveParent);

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/expenses/categories");

        // Assert
        var categories = await ApiTenant.ReadDataAsync<List<GetAllExpenseCategoriesResponse>>(response);
        categories.Select(c => c.Name).ShouldBe(["Administrativo", "Operacional"]);
        categories[1].Subcategories.Select(s => s.Name).ShouldBe(["Insumos", "Limpeza"]);
    }

    [Fact]
    public async Task GivenActiveSubcategory_WhenCreatingExpense_ThenReturns201AndPersistsItForTenant()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var command = new CreateExpenseCommand(subcategory.Id, 150m, DueDate, "Distribuidora Sul", "Farinha", null);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/expenses", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateExpenseResponse>(response);
        response.Headers.Location.ShouldBe(new Uri($"/api/v1/expenses/{created.ExpenseId}", UriKind.Relative));
        var persisted = await LoadExpenseAsync(tenant, created.ExpenseId);
        persisted.EstablishmentId.ShouldBe(tenant.EstablishmentId);
        persisted.Amount.ShouldBe(150m);
        persisted.Status.ShouldBe(ExpenseStatus.Pending);
    }

    [Fact]
    public async Task GivenPendingExpense_WhenUpdating_ThenReturns204AndPersistsChanges()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var expense = await SeedExpenseAsync(tenant, subcategory, "Farinha");
        var request = new { CategoryId = (Guid?)null, Amount = 300m, DueDate = DueDate, Supplier = "Atacadão", Description = "Embalagens" };

        // Act
        var response = await tenant.Client.PutAsJsonAsync($"/api/v1/expenses/{expense.Id}", request, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await LoadExpenseAsync(tenant, expense.Id);
        persisted.Amount.ShouldBe(300m);
        persisted.Supplier.ShouldBe("Atacadão");
        persisted.Description.ShouldBe("Embalagens");
    }

    [Fact]
    public async Task GivenPendingExpense_WhenMarkingAsPaid_ThenReturns204AndStoresPaymentDate()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var expense = await SeedExpenseAsync(tenant, subcategory, "Farinha");

        // Act
        var response = await tenant.Client.PatchAsJsonAsync($"/api/v1/expenses/{expense.Id}/mark-as-paid",
            new { PaymentDate = DueDate }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await LoadExpenseAsync(tenant, expense.Id);
        persisted.Status.ShouldBe(ExpenseStatus.Paid);
        persisted.PaymentDate.ShouldBe(DueDate);
    }

    [Fact]
    public async Task GivenCancelledExpense_WhenMarkingAsPaid_ThenReturns422WithRuleMessage()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var expense = await SeedExpenseAsync(tenant, subcategory, "Farinha", e => e.AsCancelled());

        // Act
        var response = await tenant.Client.PatchAsJsonAsync($"/api/v1/expenses/{expense.Id}/mark-as-paid",
            new { PaymentDate = DueDate }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ApiTenant.ReadProblemAsync(response)).Detail.ShouldBe("Não é possível pagar uma despesa cancelada.");
    }

    [Fact]
    public async Task GivenExistingExpense_WhenDeleting_ThenReturns204AndRemovesIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        var expense = await SeedExpenseAsync(tenant, subcategory, "Farinha");

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/v1/expenses/{expense.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await tenant.WithDbContextAsync(db => db.Expenses.AnyAsync(e => e.Id == expense.Id))).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenParentCategory_WhenCreatingSubcategory_ThenReturns201AndNestsItUnderParent()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var parent = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(parent);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/expenses/categories",
            new CreateCategoryCommand("Bebidas", parent.Id), ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateCategoryResponse>(response);
        response.Headers.Location.ShouldBe(new Uri(
            $"/api/v1/expenses/categories/{created.CategoryId}", UriKind.Relative));
        var persisted = await tenant.WithDbContextAsync(db => db.ExpenseCategories.SingleAsync(c => c.Id == created.CategoryId));
        persisted.ParentCategoryId.ShouldBe(parent.Id);
        persisted.EstablishmentId.ShouldBe(tenant.EstablishmentId);
    }

    [Fact]
    public async Task GivenCategory_WhenRenaming_ThenReturns204AndPersistsName()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var category = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Operacional").Build();
        await tenant.SeedAsync(category);

        // Act
        var response = await tenant.Client.PutAsJsonAsync($"/api/v1/expenses/categories/{category.Id}",
            new { Name = "Operação", IsActive = (bool?)null }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await tenant.WithDbContextAsync(db => db.ExpenseCategories.SingleAsync(c => c.Id == category.Id)))
            .Name.ShouldBe("Operação");
    }

    [Fact]
    public async Task GivenCategoryWithExpenses_WhenDeleting_ThenReturns422AndKeepsItActive()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (_, subcategory) = await SeedCategoriesAsync(tenant);
        await SeedExpenseAsync(tenant, subcategory, "Farinha");

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/v1/expenses/categories/{subcategory.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await tenant.WithDbContextAsync(db => db.ExpenseCategories.SingleAsync(c => c.Id == subcategory.Id)))
            .IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenCategoryWithoutExpenses_WhenDeleting_ThenReturns204AndDeactivatesItWithSubcategories()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var (parent, subcategory) = await SeedCategoriesAsync(tenant);

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/v1/expenses/categories/{parent.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await tenant.WithDbContextAsync(db => db.ExpenseCategories
            .Where(c => c.Id == parent.Id || c.Id == subcategory.Id).ToListAsync());
        persisted.ShouldAllBe(c => !c.IsActive);
    }

    private static async Task<(Category Parent, Category Subcategory)> SeedCategoriesAsync(ApiTenant tenant)
    {
        var parent = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Operacional").Build();
        var subcategory = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName("Insumos")
            .WithParent(parent).Build();
        await tenant.SeedAsync(parent, subcategory);
        return (parent, subcategory);
    }

    private static async Task<Expense> SeedExpenseAsync(ApiTenant tenant, Category category, string description,
        Func<ExpenseBuilder, ExpenseBuilder>? configure = null)
    {
        var builder = new ExpenseBuilder().WithEstablishmentId(tenant.EstablishmentId).WithCategoryId(category.Id)
            .WithDescription(description);
        var expense = (configure?.Invoke(builder) ?? builder).Build();
        await tenant.SeedAsync(expense);
        return expense;
    }

    private static Task<Expense> LoadExpenseAsync(ApiTenant tenant, Guid expenseId) =>
        tenant.WithDbContextAsync(db => db.Expenses.SingleAsync(e => e.Id == expenseId));
}
