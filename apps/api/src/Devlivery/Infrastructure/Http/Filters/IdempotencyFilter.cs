namespace Devlivery.Infrastructure.Http.Filters;

internal sealed record IdempotencyEndpointMetadata(
    Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> Execute);

internal sealed class IdempotencyFilter : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) =>
        context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IdempotencyEndpointMetadata>() is { } metadata
            ? metadata.Execute(context, next)
            : next(context);
}
