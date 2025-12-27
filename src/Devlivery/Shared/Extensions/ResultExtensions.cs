using Devlivery.Shared.Infrastructure.WebServer.Models;

using FluentResults;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Shared.Extensions;

/// <summary>
/// Extension methods for converting FluentResults to standardized API responses
/// </summary>
public static class ResultExtensions
{

    /// <summary>
    /// Converts a Result to an Ok (ApiResponse of T) response (200)
    /// </summary>
    public static Ok<ApiResponse<T>> ToOk<T>(this Result<T> result)
    {
        return TypedResults.Ok(ApiResponse<T>.Success(result.Value));
    }

    /// <summary>
    /// Converts a Result to a Created (ApiResponse of T) response (201)
    /// </summary>
    public static Created<ApiResponse<T>> ToCreated<T>(this Result<T> result, string uri)
    {
        return TypedResults.Created(uri, ApiResponse<T>.Success(result.Value));
    }

    /// <summary>
    /// Converts a Result to a NoContent response (204)
    /// </summary>
    public static NoContent ToNoContent(this Result _)
    {
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Converts a Result with generic type to a NoContent response (204)
    /// </summary>
    public static NoContent ToNoContent<T>(this Result<T> _)
    {
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Converts a failed Result to a NotFound (ApiResponse) response (404)
    /// </summary>
    public static NotFound<ApiResponse<T>> ToNotFound<T>(this Result<T> result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.NotFound(ApiResponse<T>.Failure(errors));
    }

    /// <summary>
    /// Converts a failed Result to a NotFound (ApiResponse) response (404)
    /// </summary>
    public static NotFound<ApiResponse> ToNotFound(this Result result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.NotFound(ApiResponse.Failure(errors));
    }

    /// <summary>
    /// Converts a failed Result to a BadRequest (ApiResponse) response (400)
    /// </summary>
    public static BadRequest<ApiResponse<T>> ToBadRequest<T>(this Result<T> result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.BadRequest(ApiResponse<T>.Failure(errors));
    }

    /// <summary>
    /// Converts a failed Result to a BadRequest (ApiResponse) response (400)
    /// </summary>
    public static BadRequest<ApiResponse> ToBadRequest(this Result result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.BadRequest(ApiResponse.Failure(errors));
    }

    /// <summary>
    /// Converts a failed Result to a Conflict (ApiResponse) response (409)
    /// </summary>
    public static Conflict<ApiResponse<T>> ToConflict<T>(this Result<T> result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.Conflict(ApiResponse<T>.Failure(errors));
    }

    /// <summary>
    /// Converts a failed Result to a Conflict (ApiResponse) response (409)
    /// </summary>
    public static Conflict<ApiResponse> ToConflict(this Result result)
    {
        var errors = result.GetErrorMessages();
        return TypedResults.Conflict(ApiResponse.Failure(errors));
    }

    public static IError? GetError(this Result result) => result.Errors.FirstOrDefault();

    public static IError? GetError<T>(this Result<T> result) => result.Errors.FirstOrDefault();

    public static string[] GetErrorMessages(this Result result)
        => [.. result.Errors.Select(e => e.Metadata.GetValueOrDefault("Errors")).OfType<string[]>().SelectMany(e => e)];

    public static string[] GetErrorMessages<T>(this Result<T> result)
        => [.. result.Errors.Select(e => e.Metadata.GetValueOrDefault("Errors")).OfType<string[]>().SelectMany(e => e)];
}