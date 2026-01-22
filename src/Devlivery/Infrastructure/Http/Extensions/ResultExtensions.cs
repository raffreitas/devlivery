using System.Net;

using Devlivery.Common.Errors;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Infrastructure.Http.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(
        this Result<T> result,
        HttpStatusCode? statusCode = null)
    {
        return result.IsSuccess
            ? HandleSuccess(result.Value, statusCode)
            : HandleFailure(result.Errors);
    }

    public static IResult ToApiResult<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess(result.Value) : HandleFailure(result.Errors);
    }

    public static IResult ToApiResult(
        this Result result,
        Func<IResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess() : HandleFailure(result.Errors);
    }

    private static IResult HandleFailure(IReadOnlyList<IError> errors)
    {
        if (errors.Count == 0)
            return Results.BadRequest(ApiResponse.Failure("Unknown error occurred"));

        var firstError = errors[0];

        var errorMessages = errors.Select(e => e.Message).ToArray();
        var response = ApiResponse.Failure(errorMessages);

        return firstError switch
        {
            NotFoundError => TypedResults.NotFound(response),
            ValidationError => TypedResults.UnprocessableEntity(response),
            UnauthorizedError => TypedResults.Unauthorized(),
            ForbiddenError _ => TypedResults.Forbid(),
            _ => TypedResults.InternalServerError()
        };
    }

    private static IResult HandleSuccess<T>(T value, HttpStatusCode? statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK => TypedResults.Ok(ApiResponse<T>.Success(value)),
            HttpStatusCode.Created => TypedResults.Created(string.Empty, ApiResponse<T>.Success(value)),
            HttpStatusCode.NoContent => TypedResults.NoContent(),
            _ => Results.Ok(ApiResponse<T>.Success(value))
        };
    }
}