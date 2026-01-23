using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResource<GetOrderByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid id, ISender sender, CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToOk();
    }
}