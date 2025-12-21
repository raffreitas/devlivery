using Devlivery.Shared.Infrastructure.Persistence.Context;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public sealed class GetAllExpenseCategoriesHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetAllExpenseCategoriesQuery, List<GetAllExpenseCategoriesResponse>>
{
    public async ValueTask<List<GetAllExpenseCategoriesResponse>> Handle(GetAllExpenseCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new GetAllExpenseCategoriesResponse(
                c.Id,
                c.Name,
                c.IsActive,
                c.SubCategories
                    .Where(sc => sc.IsActive)
                    .OrderBy(sc => sc.Name)
                    .Select(sc => new GetAllExpenseCategoriesResponse(
                        sc.Id,
                        sc.Name,
                        sc.IsActive,
                        Array.Empty<GetAllExpenseCategoriesResponse>()))
                    .ToArray()))
            .ToListAsync(cancellationToken);

        return categories;
    }
}