using Devlivery.Common.Exceptions;
using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Http.ExceptionHandlers;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client errors (4xx) are expected outcomes, not server failures: keep them out of error-level alerts.
        if (exception is DomainException or BadHttpRequestException or UnauthorizedAccessException
            or UnauthorizedException or DbUpdateConcurrencyException)
            logger.LogWarning(exception, "Requisição rejeitada: {Reason}", exception.Message);
        else
            logger.LogError(exception, "Houve um erro não tratado ao processar a requisição.");

        switch (exception)
        {
            case DomainException domainException:
                httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await httpContext.Response.WriteAsJsonAsync(
                    new ApiProblemDetails
                    {
                        Title = "Requisição inválida",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = domainException.Message
                    }, cancellationToken: cancellationToken);
                return true;
            case BadHttpRequestException badRequest:
                // Malformed input that could not be bound (e.g. an invalid date in the query string or invalid JSON).
                httpContext.Response.StatusCode = badRequest.StatusCode;
                await httpContext.Response.WriteAsJsonAsync(
                    new ApiProblemDetails
                    {
                        Title = "Requisição inválida",
                        Status = badRequest.StatusCode,
                        Detail = "Os dados enviados na requisição estão em um formato inválido."
                    }, cancellationToken: cancellationToken);
                return true;
            case UnauthorizedAccessException:
            case UnauthorizedException:
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(
                    new ApiProblemDetails
                    {
                        Title = "Acesso não autorizado",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = "Acesso não autorizado."
                    }, cancellationToken: cancellationToken);
                return true;
            case DbUpdateConcurrencyException:
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(
                    new ApiProblemDetails
                    {
                        Title = "Conflito de atualização",
                        Status = StatusCodes.Status409Conflict,
                        Detail = "O registro foi alterado por outra operação. Atualize os dados e tente novamente."
                    }, cancellationToken: cancellationToken);
                return true;
            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(
                    new ApiProblemDetails
                    {
                        Title = "Erro Interno do Servidor",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = "Houve um erro inesperado ao processar a requisição."
                    }, cancellationToken);

                return true;
        }
    }
}