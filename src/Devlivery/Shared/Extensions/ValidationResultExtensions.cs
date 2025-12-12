using FluentValidation.Results;

namespace Devlivery.Shared.Extensions;

public static class ValidationResultExtensions
{
    public static Dictionary<string, string[]> GetErrors(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key.ToLower(),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return errors;
    }
}