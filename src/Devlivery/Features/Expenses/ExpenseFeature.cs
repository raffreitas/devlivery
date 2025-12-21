using Devlivery.Features.Expenses.Commands.CreateExpense;
using Devlivery.Features.Expenses.Commands.DeleteExpense;
using Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;
using Devlivery.Features.Expenses.Commands.UpdateExpense;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Features.Expenses.Infrastructure;
using Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Features.Expenses.Queries.GetExpenseById;

namespace Devlivery.Features.Expenses;

public static class ExpenseFeature
{
    public static IServiceCollection AddExpenseFeature(this IServiceCollection services)
    {
        // Register Repositories
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var expensesGroup = app.MapGroup("/api/expenses").WithTags("Expenses");

        // Expense CRUD endpoints
        CreateExpenseEndpoint.MapEndpoint(expensesGroup);
        UpdateExpenseEndpoint.MapEndpoint(expensesGroup);
        DeleteExpenseEndpoint.MapEndpoint(expensesGroup);
        MarkExpenseAsPaidEndpoint.MapEndpoint(expensesGroup);
        GetAllExpensesEndpoint.MapEndpoint(expensesGroup);
        GetExpenseByIdEndpoint.MapEndpoint(expensesGroup);
        GetAllExpenseCategoriesEndpoint.MapEndpoint(expensesGroup);

        return app;
    }
}