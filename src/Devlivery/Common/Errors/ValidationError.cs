using Devlivery.Common.Validation;

using FluentResults;

namespace Devlivery.Common.Errors;

public sealed class ValidationError : Error
{
    private const string DefaultMessage = "Um ou mais erros de validação ocorreram.";
    public const string Name = nameof(ValidationError);
    public ValidationFailure[] Errors { get; } = [];

    public ValidationError(string? message = null) : base(message ?? DefaultMessage)
    {
    }

    public ValidationError(ValidationFailure[] error) : base(DefaultMessage)
    {
        Errors = error;
    }
}