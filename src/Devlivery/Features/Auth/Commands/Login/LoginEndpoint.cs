using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

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

    private static async Task<IResult> Handle(LoginCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.ToApiResult();
    }
}