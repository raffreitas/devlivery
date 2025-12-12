using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>
{
    public ValidationResult Validate() => new Validator().Validate(this);
};

public sealed class Validator : AbstractValidator<LoginCommand>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .EmailAddress().WithMessage("O campo '{PropertyName}' deve ser um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MinimumLength(6).WithMessage("O campo '{PropertyName}' deve ter no mínimo {MinLength} caracteres.");
    }
}