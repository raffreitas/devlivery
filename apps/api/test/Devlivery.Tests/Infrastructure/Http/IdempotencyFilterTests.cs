using System.Net;
using System.Net.Http.Json;

using Devlivery.Common.Endpoints;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Tests.Common.Api;

using FluentResults;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class IdempotencyFilterTests(ApiFactory api)
{
    [Fact]
    public async Task GivenSharedFilter_WhenAnotherEndpointOptsIn_ThenTheSameKeyIsIsolatedByOperationAndScope()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        using var host = api.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IEndpoint, ProbeEndpoint>()));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = tenant.Client.DefaultRequestHeaders.Authorization;
        var requestId = Guid.NewGuid();
        var scopeId = Guid.NewGuid();

        // Act
        var missingKey = await Post(client, null, "reserve", scopeId);
        var first = await Post(client, requestId, "reserve", scopeId);
        var replay = await Post(client, requestId, "reserve", scopeId);
        var otherOperation = await Post(client, requestId, "release", scopeId);
        var otherScope = await Post(client, requestId, "reserve", Guid.NewGuid());

        // Assert
        missingKey.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        replay.StatusCode.ShouldBe(HttpStatusCode.OK);
        otherOperation.StatusCode.ShouldBe(HttpStatusCode.OK);
        otherScope.StatusCode.ShouldBe(HttpStatusCode.OK);
        var recorded = await ApiTenant.ReadDataAsync<ProbeResponse>(first);
        (await ApiTenant.ReadDataAsync<ProbeResponse>(replay)).ShouldBe(recorded);
        (await ApiTenant.ReadDataAsync<ProbeResponse>(otherOperation)).Id.ShouldNotBe(recorded.Id);
        (await ApiTenant.ReadDataAsync<ProbeResponse>(otherScope)).Id.ShouldNotBe(recorded.Id);
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, Guid? requestId, string operation, Guid scopeId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/idempotency-probe/{operation}/{scopeId}")
        {
            Content = JsonContent.Create(new ProbeRequest("test content"))
        };
        if (requestId.HasValue) request.Headers.Add("Idempotency-Key", requestId.Value.ToString());
        return await client.SendAsync(request);
    }

    private sealed class ProbeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("idempotency-probe/reserve/{scopeId:guid}", Handle)
                .WithIdempotency<ProbeRequest, ProbeResponse>("probe-reserve", Bind);
            app.MapPost("idempotency-probe/release/{scopeId:guid}", Handle)
                .WithIdempotency<ProbeRequest, ProbeResponse>("probe-release", Bind);
        }

        private static Result<ProbeResponse> Handle(Guid scopeId, ProbeRequest request) =>
            Result.Ok(new ProbeResponse(Guid.NewGuid(), scopeId, request.Message));

        private static IdempotencyEndpointRequest<ProbeRequest> Bind(EndpointFilterInvocationContext context)
        {
            var request = context.GetArgument<ProbeRequest>(1);
            return new(request, context.GetArgument<Guid>(0), request.Message);
        }
    }

    private sealed record ProbeRequest(string Message);
    private sealed record ProbeResponse(Guid Id, Guid ScopeId, string Message);
}
