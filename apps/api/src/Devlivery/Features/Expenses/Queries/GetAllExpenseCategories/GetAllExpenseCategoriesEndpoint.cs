using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenseCategories;

public static class GetAllExpenseCategoriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", Handle)
            .Produces<ApiResponse<List<GetAllExpenseCategoriesResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<List<GetAllExpenseCategoriesResponse>>>> Handle(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetAllExpenseCategoriesQuery();
        var result = await sender.Send(query, ct);

        var response = ApiResponse<List<GetAllExpenseCategoriesResponse>>.Success(result);

        return TypedResults.Ok(response);
    }
}