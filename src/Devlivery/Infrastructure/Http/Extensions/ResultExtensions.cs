using System.Net;

using Devlivery.Common.Errors;
using Devlivery.Common.Pagination;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Infrastructure.Http.Extensions;

public static class ResultExtensions
{
    public static IResult ToOk<T>(this Result<PaginatedResult<T>> result)
    {
        if (result.IsFailed) return result.ToError();

        var paged = result.Value;
        var resource = new ApiResource<T[]>(paged.Items, Metadata.FromPaginationResult(paged));

        return TypedResults.Ok(resource);
    }

    public static IResult ToOk<T>(this Result<T> result) where T : notnull
    {
        return result.IsFailed
            ? result.ToError()
            : TypedResults.Ok(new ApiResource<T>(result.Value));
    }

    public static IResult ToCreated(this Result result)
    {
        return result.IsFailed
            ? result.ToError()
            : TypedResults.Created();
    }

    public static IResult ToCreated<T>(this Result<T> result, Func<T, string>? locationFactory = null)
        where T : notnull
    {
        if (result.IsFailed)
            return result.ToError();

        string resourceUri = locationFactory is not null
            ? locationFactory.Invoke(result.Value)
            : string.Empty;

        return TypedResults.Created(resourceUri, new ApiResource<T>(result.Value));
    }

    public static IResult ToNoContent(this Result result)
    {
        return result.IsFailed ? result.ToError() : TypedResults.NoContent();
    }

    private static ProblemHttpResult ToError(this IResultBase result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Não é possível converter um resultado bem-sucedido em um erro.");

        var error = result.Errors[0];

        (HttpStatusCode statusCode, string title) = error switch
        {
            NotFoundError => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            ValidationError => (HttpStatusCode.UnprocessableEntity, "Requisição inválida"),
            UnauthorizedError => (HttpStatusCode.Unauthorized, "Acesso não autorizado"),
            ForbiddenError => (HttpStatusCode.Forbidden, "Acesso proibido"),
            ConflictError => (HttpStatusCode.Conflict, "Conflito de recurso"),
            _ => (HttpStatusCode.BadRequest, "Requisição inválida")
        };

        var problemDetails = new ApiProblemDetails
        {
            Title = title,
            Detail = error.Message,
            Status = (int)statusCode,
            Errors = error is ValidationError err && err.Errors.Length != 0
                ? err.Errors.DistinctBy(e => e.Field)
                    .ToDictionary(e => $"{char.ToLower(e.Field[0])}{e.Field[1..]}", e => e.Message)
                : null
        };
        return TypedResults.Problem(problemDetails);
    }
}