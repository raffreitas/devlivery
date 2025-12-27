using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public static class GetExpenseByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{expenseId:guid}", Handle)
            .Produces<ApiResponse<GetExpenseByIdResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async
        Task<Results<Ok<ApiResponse<GetExpenseByIdResponse>>, BadRequest<ApiResponse<GetExpenseByIdResponse>>>> Handle(
            Guid expenseId,
            ISender sender,
            CancellationToken ct)
    {
        var query = new GetExpenseByIdQuery(expenseId);
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToBadRequest();
    }
}