using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public sealed record GetAllExpenseCategoriesQuery : IQuery<Result<List<GetAllExpenseCategoriesResponse>>>;