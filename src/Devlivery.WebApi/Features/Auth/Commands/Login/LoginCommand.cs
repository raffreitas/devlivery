using FluentValidation;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password);

public class Validator : AbstractValidator<LoginCommand>
{
    public Validator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}