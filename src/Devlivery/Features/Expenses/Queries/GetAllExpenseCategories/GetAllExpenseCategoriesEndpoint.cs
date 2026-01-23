using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

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
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetAllExpenseCategoriesQuery();
        var result = await sender.Send(query, ct);
        return result.ToOk();
    }
}