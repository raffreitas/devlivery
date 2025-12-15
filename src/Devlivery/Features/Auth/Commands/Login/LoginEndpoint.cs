using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Handle)
            .Produces<ApiResponse<LoginResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }

    private static async Task<Results<Ok<ApiResponse<LoginResponse>>, BadRequest<ApiResponse<LoginResponse>>,
        UnauthorizedHttpResult>> Handle(
        LoginCommand command,
        ISender sender,
        CancellationToken ct
    )
    {
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                _ => TypedResults.Unauthorized()
            };
    }
}