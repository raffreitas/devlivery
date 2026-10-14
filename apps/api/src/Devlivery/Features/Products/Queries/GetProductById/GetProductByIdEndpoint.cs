using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Products.Queries.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResource<GetProductByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>> handler,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);
        return result.ToOk();
    }
}