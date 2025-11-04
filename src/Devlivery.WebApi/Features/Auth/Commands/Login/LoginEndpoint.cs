using FluentValidation;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (IValidator<LoginCommand> validator, LoginCommand command,
                LoginHandler handler, CancellationToken ct) =>
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await handler.HandleAsync(command, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Unauthorized();
            })
            .WithTags("Auth")
            .WithName("Login")
            .Produces<LoginResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}