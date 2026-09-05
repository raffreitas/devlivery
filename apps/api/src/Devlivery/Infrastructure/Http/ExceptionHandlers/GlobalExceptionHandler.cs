using Devlivery.Common.Exceptions;
using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Diagnostics;

namespace Devlivery.Infrastructure.Http.ExceptionHandlers;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
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