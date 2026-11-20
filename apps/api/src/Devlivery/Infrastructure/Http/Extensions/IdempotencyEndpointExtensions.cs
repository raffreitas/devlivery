using Devlivery.Common.Errors;
using Devlivery.Common.Validation;
using Devlivery.Infrastructure.Http.Filters;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Infrastructure.Persistence.Idempotency;

using FluentResults;

using FluentValidation;

namespace Devlivery.Infrastructure.Http.Extensions;

public sealed record IdempotencyEndpointRequest<TCommand>(
    TCommand Command, Guid ScopeId, string CanonicalContent);

public static class IdempotencyEndpointExtensions
{
    /// <summary>Enables idempotency for an endpoint that returns Result&lt;TResponse&gt; and stages its writes in the shared unit of work.</summary>
    public static RouteHandlerBuilder WithIdempotency<TCommand, TResponse>(
        this RouteHandlerBuilder endpoint,
        string operation,
        Func<EndpointFilterInvocationContext, IdempotencyEndpointRequest<TCommand>> bindRequest,
        Func<Result<TResponse>, IResult>? onFirstSuccess = null) where TResponse : notnull =>
        endpoint.WithMetadata(new IdempotencyEndpointMetadata(
            (context, next) => ExecuteAsync(operation, bindRequest(context), context, next, onFirstSuccess)));

    private static async ValueTask<object?> ExecuteAsync<TCommand, TResponse>(
        string operation,
        IdempotencyEndpointRequest<TCommand> request,
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next,
        Func<Result<TResponse>, IResult>? onFirstSuccess) where TResponse : notnull
    {
        var http = context.HttpContext;
        var services = http.RequestServices;
        var ct = http.RequestAborted;

        // Validate new requests and replays with the same command rules.
        var failures = new List<ValidationFailure>();
        foreach (var validator in services.GetServices<IValidator<TCommand>>())
        {
            var validation = await validator.ValidateAsync(request.Command, ct);
            failures.AddRange(validation.Errors.Select(error => new ValidationFailure(error.PropertyName, error.ErrorMessage)));
        }

        var requestId = ReadRequestId(http.Request.Headers, failures);
        if (failures.Count > 0)
            return Result.Fail<TResponse>(new ValidationError([.. failures])).ToOk();

        var actor = await services.GetRequiredService<ICurrentUserAccessor>().ResolveAsync(ct);
        var store = services.GetRequiredService<IdempotencyStore>();
        var key = new IdempotencyKey(operation, request.ScopeId, requestId);
        var result = await store.ExecuteAsync(key, actor.Id, request.CanonicalContent, async _ =>
            await next(context) is Result<TResponse> endpointResult
                ? endpointResult
                : throw new InvalidOperationException("An endpoint configured with WithIdempotency must return Result<TResponse>."),
            ct);

        var response = result.Map(value => value.Response);
        return result.IsSuccess && !result.Value.Replayed && onFirstSuccess is not null
            ? onFirstSuccess(response)
            : response.ToOk();
    }

    private static Guid ReadRequestId(IHeaderDictionary headers, List<ValidationFailure> failures)
    {
        var values = headers[IdempotencyFilter.HeaderName];
        if (values.Count == 1 && Guid.TryParse(values[0], out var requestId) && requestId != Guid.Empty)
            return requestId;

        failures.Add(new ValidationFailure("requestId", "Informe um único UUID válido no cabeçalho Idempotency-Key."));
        return Guid.Empty;
    }
}
