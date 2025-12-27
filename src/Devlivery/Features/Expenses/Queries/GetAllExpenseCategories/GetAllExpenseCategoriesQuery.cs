using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public sealed record GetAllExpenseCategoriesQuery : IQuery<List<GetAllExpenseCategoriesResponse>>;