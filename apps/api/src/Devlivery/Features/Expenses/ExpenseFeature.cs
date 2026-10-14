using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Infrastructure.Persistence.Repositories;

namespace Devlivery.Features.Expenses;

public static class ExpenseFeature
{
    public static IServiceCollection AddExpenseFeature(this IServiceCollection services)
    {
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}