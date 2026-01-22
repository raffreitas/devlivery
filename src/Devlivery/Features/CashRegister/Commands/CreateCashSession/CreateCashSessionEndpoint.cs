using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public static class CreateCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sessions", Handle)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(CreateCashSessionCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.ToApiResult(data =>
            TypedResults.Created($"/api/cash-register/sessions/{data.Id}",
                ApiResponse<CreateCashSessionResponse>.Success(data)));
    }
}