using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Handle)
            .WithTags("Auth")
            .WithName("Login")
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<Ok<ApiResponse<LoginResponse>>, ValidationProblem, UnauthorizedHttpResult>> Handle(
        LoginCommand command,
        IValidator<LoginCommand> validator,
        LoginHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? result.ToOk("Login successful")
            : TypedResults.Unauthorized();
    }
}