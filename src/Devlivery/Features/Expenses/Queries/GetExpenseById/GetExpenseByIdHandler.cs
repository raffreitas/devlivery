using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetExpenseByIdQuery, Result<GetExpenseByIdResponse>>
{
    public async ValueTask<Result<GetExpenseByIdResponse>> Handle(GetExpenseByIdQuery query,
        CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.Id == query.ExpenseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (expense == null)
        {
            return Result.Fail<GetExpenseByIdResponse>(new NotFoundError("Despesa não encontrada."));
        }

        var category = await dbContext.ExpenseCategories.AsNoTracking()
            .SingleAsync(x => x.Id == expense.CategoryId, cancellationToken);

        return new GetExpenseByIdResponse(
            expense.Id,
            new CategoryDto(
                category.Id,
                category.Name,
                category.IsActive,
                category.Subcategories
                    .Select(sc => new CategoryDto(
                        sc.Id,
                        sc.Name,
                        sc.IsActive,
                        []))
                    .ToArray()),
            expense.Supplier,
            expense.Description,
            expense.Amount,
            expense.DueDate,
            expense.PaymentDate,
            expense.Status.ToString(),
            expense.CreatedAt,
            expense.UpdatedAt
        );
    }
}