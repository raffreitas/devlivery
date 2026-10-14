using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public static class GetAllExpenseCategoriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", Handle)
            .Produces<ApiResource<List<GetAllExpenseCategoriesResponse>>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        IQueryHandler<GetAllExpenseCategoriesQuery, Result<List<GetAllExpenseCategoriesResponse>>> handler,
        CancellationToken ct)
    {
        var query = new GetAllExpenseCategoriesQuery();
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}