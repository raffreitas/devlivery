using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public static class CreateCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Created<ApiResponse<CreateCashSessionResponse>>,
        BadRequest<ApiResponse<CreateCashSessionResponse>>, Conflict<ApiResponse<CreateCashSessionResponse>>>> Handle(
        CreateCashSessionCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/cash-sessions/{result.Value.Id}")
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                DomainRuleError => result.ToConflict(),
                _ => result.ToBadRequest()
            };
    }
}