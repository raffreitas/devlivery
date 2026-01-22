using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/active", Handle)
            .Produces<ApiResource<GetActiveCashSessionResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(ISender sender, CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await sender.Send(query, ct);
        return result.ToOk();
    }
}