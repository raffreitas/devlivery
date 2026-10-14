using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("expenses/{expenseId:guid}", Handle)
            .WithTags(Tags.Expenses)
            .Produces<ApiResource<GetExpenseByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid expenseId,
        IQueryHandler<GetExpenseByIdQuery, Result<GetExpenseByIdResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetExpenseByIdQuery(expenseId);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}