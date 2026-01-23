using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResource<List<GetAllProductsResponse>>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(ISender sender, CancellationToken ct)
    {
        var query = new GetAllProductsQuery();
        var result = await sender.Send(query, ct);
        return result.ToOk();
    }
}