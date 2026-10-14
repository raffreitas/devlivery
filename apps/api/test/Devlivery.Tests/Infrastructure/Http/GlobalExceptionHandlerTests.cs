using System.Net;
using System.Net.Http.Json;
using System.Text;

using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

[Trait("Category", "Unit Tests")]
public sealed class GlobalExceptionHandlerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=unused");
            // Development throws on binding failures instead of returning 400 directly; reproduce it here.
            builder.ConfigureServices(services =>
                services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true));
        });

    [Fact]
    public async Task Should_Return_400_With_ProblemDetails_When_Request_Cannot_Be_Bound()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var malformedJson = new StringContent("{ \"email\": ", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", malformedJson);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();
        problem!.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Title.ShouldBe("Requisição inválida");
    }

    public void Dispose() => _factory.Dispose();
}