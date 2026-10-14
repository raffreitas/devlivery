namespace Devlivery.Common.Endpoints;

/// <summary>
/// An HTTP endpoint discovered automatically from the assembly and mapped under the <c>/api</c> group.
/// Each endpoint declares its full route (relative to <c>/api</c>) and its OpenAPI tag.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}