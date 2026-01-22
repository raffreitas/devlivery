using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Diagnostics;

namespace Devlivery.Infrastructure.Http;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var response = ApiResponse.Failure("Houve um erro inesperado ao processar a requisição.");
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}