using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .EmailAddress().WithMessage("O campo '{PropertyName}' deve ser um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MinimumLength(6).WithMessage("O campo '{PropertyName}' deve ter no mínimo {MinLength} caracteres.");
    }
}