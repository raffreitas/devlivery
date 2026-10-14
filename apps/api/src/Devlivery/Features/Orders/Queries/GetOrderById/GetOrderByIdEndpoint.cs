using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResource<GetOrderByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IQueryHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}