using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.WebApi.Shared.Extensions;

/// <summary>
/// Extension methods for handling validation errors using Problem Details
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Converts FluentValidation ValidationResult to ValidationProblem (400)
    /// </summary>
    public static ValidationProblem ToValidationProblem(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key.ToLower(),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return TypedResults.ValidationProblem(
            errors,
            title: "Um ou mais erros de validação ocorreram",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
}