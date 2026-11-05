using Devlivery.Infrastructure.Http.Models;

namespace Devlivery.Infrastructure.Http.Filters;

public sealed class PaginationValidationFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var pagination = context.Arguments.OfType<PaginationParameters>().FirstOrDefault();
        if (pagination is null || IsValid(pagination)) return next(context);

        return ValueTask.FromResult<object?>(TypedResults.Problem(new ApiProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Requisição inválida",
            Detail = "Os dados enviados na requisição estão em um formato inválido."
        }));
    }

    private static bool IsValid(PaginationParameters pagination) =>
        pagination is
        {
            Page: >= 1,
            PageSize: >= 1 and <= PaginationParameters.MaximumPageSize
        } && (long)(pagination.Page - 1) * pagination.PageSize <= int.MaxValue;
}
