using Devlivery.WebApi.Shared.Models;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Shared.Extensions;

/// <summary>
/// Extension methods for converting FluentResults to standardized API responses using Problem Details
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an Ok<T> response (200)
    /// </summary>
    public static Ok<ApiResponse<T>> ToOk<T>(this Result<T> result, string? message = null)
    {
        return TypedResults.Ok(ApiResponse<T>.Ok(result.Value, message));
    }

    /// <summary>
    /// Converts a Result to a Created<T> response (201)
    /// </summary>
    public static Created<ApiResponse<T>> ToCreated<T>(this Result<T> result, string uri, string? message = null)
    {
        return TypedResults.Created(uri, ApiResponse<T>.Ok(result.Value, message));
    }

    /// <summary>
    /// Converts a Result to a NoContent response (204)
    /// </summary>
    public static NoContent ToNoContent(this Result result)
    {
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Converts a Result with generic type to a NoContent response (204)
    /// </summary>
    public static NoContent ToNoContent<T>(this Result<T> result)
    {
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Converts a failed Result to a NotFound ProblemDetails (404)
    /// </summary>
    public static NotFound<ProblemDetails> ToNotFoundProblem(this Result result)
    {
        var errorMessage = result.Errors[0]?.Message ?? "Resource not found";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource Not Found",
            Detail = errorMessage,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        };

        return TypedResults.NotFound(problemDetails);
    }

    /// <summary>
    /// Converts a failed Result to a BadRequest ProblemDetails (400)
    /// </summary>
    public static BadRequest<ProblemDetails> ToBadRequestProblem(this Result result)
    {
        var errorMessage = result.Errors[0]?.Message ?? "Bad request";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = errorMessage,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        return TypedResults.BadRequest(problemDetails);
    }

    /// <summary>
    /// Converts a failed Result with generic type to a NotFound ProblemDetails (404)
    /// </summary>
    public static NotFound<ProblemDetails> ToNotFoundProblem<T>(this Result<T> result)
    {
        var errorMessage = result.Errors[0]?.Message ?? "Resource not found";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource Not Found",
            Detail = errorMessage,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        };

        return TypedResults.NotFound(problemDetails);
    }

    /// <summary>
    /// Converts a failed Result with generic type to a BadRequest ProblemDetails (400)
    /// </summary>
    public static BadRequest<ProblemDetails> ToBadRequestProblem<T>(this Result<T> result)
    {
        var errorMessage = result.Errors[0]?.Message ?? "Bad request";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = errorMessage,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        return TypedResults.BadRequest(problemDetails);
    }
}