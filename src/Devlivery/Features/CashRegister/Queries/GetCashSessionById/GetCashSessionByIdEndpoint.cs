using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public static class GetCashSessionByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/{id:guid}", Handle)
            .Produces<ApiResource<GetCashSessionByIdResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(Guid id, ISender sender, CancellationToken ct)
    {
        var query = new GetCashSessionByIdQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToOk();
    }
}