using FluentValidation;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (IValidator<LoginCommand> validator, LoginCommand command,
                LoginHandler handler, CancellationToken ct) =>
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await handler.HandleAsync(command, ct);

                return Results.Ok(result);
            })
            .WithTags("Auth")
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}