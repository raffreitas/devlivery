using Asp.Versioning;

namespace Devlivery.Common.Endpoints;

/// <summary>
/// An HTTP endpoint discovered automatically and mapped under its versioned API group.
/// Routes are relative to <c>/api/v{version}</c>; existing endpoints default to v1.
/// </summary>
public interface IEndpoint
{
    ApiVersion Version => new(1, 0);

    void MapEndpoint(IEndpointRouteBuilder app);
}
