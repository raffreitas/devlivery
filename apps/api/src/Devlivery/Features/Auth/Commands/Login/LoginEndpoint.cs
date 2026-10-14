using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", Handle)
            .WithTags(Tags.Auth)
            .Produces<ApiResource<LoginResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("login")
            .Produces<ApiProblemDetails>(StatusCodes.Status429TooManyRequests)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        LoginCommand command,
        ICommandHandler<LoginCommand, Result<LoginResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return result.ToOk();
    }
}