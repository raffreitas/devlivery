using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Asp.Versioning;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

[Trait("Category", "Integration Tests")]
public sealed class OpenApiConfigurationTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task GivenEnvironment_WhenRequestingDocumentation_ThenExposesItOnlyInDevelopment(string environment)
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        // Only endpoint metadata is exercised; handlers are registered for parameter inference.
        builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);
        builder.Services.AddOpenApiConfiguration();
        builder.Services.AddHandlers();
        builder.Services.AddEndpoints();
        // A test-only endpoint verifies future version discovery without publishing a v2 in the application.
        builder.Services.AddSingleton<IEndpoint, V2Endpoint>();
        builder.Services.AddAuthorization();
        builder.Services.AddRateLimiter(_ => { });
        await using var app = builder.Build();
        app.UseOpenApiConfiguration();
        app.MapEndpoints();
        await app.StartAsync();
        using var client = app.GetTestClient();

        // Act
        var document = await client.GetAsync("/openapi/v1.json");
        var v2Document = await client.GetAsync("/openapi/v2.json");
        var scalar = await client.GetAsync("/scalar/");

        // Assert
        if (environment == "Production")
        {
            document.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            v2Document.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            scalar.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            return;
        }

        document.StatusCode.ShouldBe(HttpStatusCode.OK);
        v2Document.StatusCode.ShouldBe(HttpStatusCode.OK);
        scalar.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await scalar.Content.ReadAsStringAsync()).ShouldContain("\"url\":\"openapi/v1.json\"");
        (await scalar.Content.ReadAsStringAsync()).ShouldContain("\"url\":\"openapi/v2.json\"");
        var json = await document.Content.ReadFromJsonAsync<JsonElement>();
        var paths = json.GetProperty("paths").EnumerateObject().Select(path => path.Name).ToArray();
        paths.ShouldNotBeEmpty();
        paths.ShouldAllBe(path => path.StartsWith("/api/v1/") && !path.Contains("{version"));
        paths.ShouldContain("/api/v1/auth/login");
        paths.ShouldContain("/api/v1/products");
        paths.ShouldNotContain("/api/v1/version-probe");
        var v2Json = await v2Document.Content.ReadFromJsonAsync<JsonElement>();
        v2Json.GetProperty("paths").EnumerateObject().Select(path => path.Name)
            .ShouldBe(["/api/v2/version-probe"]);
        json.GetProperty("components").GetProperty("securitySchemes")
            .GetProperty("Bearer").GetProperty("scheme").GetString().ShouldBe("bearer");
    }

    private sealed class V2Endpoint : IEndpoint
    {
        public ApiVersion Version => new(2, 0);

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("version-probe", () => TypedResults.Ok("2.0"));
        }
    }
}
