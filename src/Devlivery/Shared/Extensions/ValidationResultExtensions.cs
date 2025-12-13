using Devlivery.Shared.Infrastructure.WebServer.Models;

using FluentValidation.Results;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Shared.Extensions;

public static class ValidationResultExtensions
{
    public static string[] GetErrors(this ValidationResult validationResult)
       => [.. validationResult.Errors.Select(e => e.ErrorMessage)];

    /// <summary>
    /// Converts FluentValidation ValidationResult to a BadRequest ApiResponse with error messages
    /// </summary>
    public static BadRequest<ApiResponse> ToBadRequest(this ValidationResult validationResult)
    {
        var errors = validationResult.GetErrors();
        return TypedResults.BadRequest(ApiResponse.Failure(errors));
    }

    /// <summary>
    /// Converts FluentValidation ValidationResult to a BadRequest ApiResponse with error messages
    /// </summary>
    public static BadRequest<ApiResponse<T>> ToBadRequest<T>(this ValidationResult validationResult)
    {
        var errors = validationResult.GetErrors();
        return TypedResults.BadRequest(ApiResponse<T>.Failure(errors));
    }
}