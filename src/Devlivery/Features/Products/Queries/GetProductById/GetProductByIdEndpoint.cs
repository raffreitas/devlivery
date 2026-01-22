using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Products.Queries.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetProductByIdResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid id, ISender sender, CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}