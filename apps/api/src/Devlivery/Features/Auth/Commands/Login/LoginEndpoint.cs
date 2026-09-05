using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Handle)
            .Produces<ApiResource<LoginResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("login")
            .Produces<ApiProblemDetails>(StatusCodes.Status429TooManyRequests)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(LoginCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.ToOk();
    }
}