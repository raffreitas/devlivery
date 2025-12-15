using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    internal sealed record Request(string Email, string Password);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Handle)
            .AllowAnonymous();
    }

    private static async Task<Results<Ok<ApiResponse<LoginResponse>>, BadRequest<ApiResponse<LoginResponse>>,
        UnauthorizedHttpResult>> Handle(
        Request request,
        ISender sender,
        CancellationToken ct
    )
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.GetError() switch
            {
                ValidationError => result.ToBadRequest(),
                UnauthorizedError => TypedResults.Unauthorized(),
                _ => TypedResults.Unauthorized()
            };
    }
}