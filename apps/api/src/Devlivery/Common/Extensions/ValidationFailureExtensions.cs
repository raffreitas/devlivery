using Devlivery.Common.Errors;

using FluentValidation.Results;

using ValidationFailure = Devlivery.Common.Validation.ValidationFailure;

namespace Devlivery.Common.Extensions;

public static class ValidationFailureExtensions
{
    public static ValidationError ToError(this Validation.Validation validation) => validation.IsFailure
        ? new ValidationError(validation.Errors)
        : throw new InvalidOperationException("Cannot create ValidationError from a successful Validation.");

    public static Validation.Validation ToFailure(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return new Validation.Validation(false, []);

        var errors = validationResult.Errors
            .Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return new Validation.Validation(true, errors);
    }
}