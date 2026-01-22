using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public static class GetExpenseByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{expenseId:guid}", Handle)
            .Produces<ApiResponse<GetExpenseByIdResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid expenseId, ISender sender, CancellationToken ct)
    {
        var query = new GetExpenseByIdQuery(expenseId);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}