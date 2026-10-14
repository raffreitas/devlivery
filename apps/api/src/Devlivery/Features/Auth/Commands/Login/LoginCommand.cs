using FluentValidation;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithName("E-mail");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithName("Senha");
    }
}