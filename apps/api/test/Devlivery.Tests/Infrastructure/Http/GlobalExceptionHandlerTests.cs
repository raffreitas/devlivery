using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

[Trait("Category", "Integration Tests")]
public sealed class GlobalExceptionHandlerTests : IDisposable
{
    private readonly CapturingLoggerProvider _logs = new();
    private readonly WebApplicationFactory<Program> _factory;

    public GlobalExceptionHandlerTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=unused");
                builder.ConfigureLogging(logging => logging.AddProvider(_logs));
                // Development throws on binding failures instead of returning 400 directly; reproduce it here.
                builder.ConfigureServices(services =>
                    services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true));
            });
    }

    public static TheoryData<Exception, int, LogLevel> HandledExceptions => new()
    {
        { new DomainException("Este caixa já está fechado."), StatusCodes.Status422UnprocessableEntity, LogLevel.Warning },
        { new BadHttpRequestException("JSON inválido"), StatusCodes.Status400BadRequest, LogLevel.Warning },
        { new UnauthorizedAccessException(), StatusCodes.Status401Unauthorized, LogLevel.Warning },
        { new DbUpdateConcurrencyException("Conflito"), StatusCodes.Status409Conflict, LogLevel.Warning },
        { new InvalidOperationException("Falha inesperada"), StatusCodes.Status500InternalServerError, LogLevel.Error }
    };

    [Fact]
    public async Task GivenMalformedJson_WhenPostingToApi_ThenReturns400ProblemDetails()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var malformedJson = new StringContent("{ \"email\": ", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/v1/auth/login", malformedJson);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();
        problem!.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Title.ShouldBe("Requisição inválida");
    }

    [Theory]
    [MemberData(nameof(HandledExceptions))]
    public async Task GivenException_WhenHandling_ThenMapsStatusCodeAndLogsClientErrorsAsWarning(
        Exception exception, int expectedStatusCode, LogLevel expectedLogLevel)
    {
        // Arrange
        var httpContext = CreateHttpContext();

        // Act
        var handled = await HandleAsync(httpContext, exception);

        // Assert
        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        _logs.Entries.ShouldHaveSingleItem().Level.ShouldBe(expectedLogLevel);
    }

    [Fact]
    public async Task GivenDomainException_WhenHandling_ThenReturnsRuleMessageAsProblemDetail()
    {
        // Arrange
        var httpContext = CreateHttpContext();

        // Act
        await HandleAsync(httpContext, new DomainException("Este caixa já está fechado."));

        // Assert
        httpContext.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ApiProblemDetails>(httpContext.Response.Body,
            JsonSerializerOptions.Web);
        problem!.Status.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        problem.Detail.ShouldBe("Este caixa já está fechado.");
    }

    public void Dispose() => _factory.Dispose();

    private static DefaultHttpContext CreateHttpContext() => new() { Response = { Body = new MemoryStream() } };

    private async Task<bool> HandleAsync(HttpContext httpContext, Exception exception)
    {
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetServices<IExceptionHandler>().Single();
        _logs.Entries.Clear();
        return await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(string Category, LogLevel Level)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, Entries);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, ConcurrentQueue<(string, LogLevel)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (category.EndsWith("GlobalExceptionHandler", StringComparison.Ordinal))
                    entries.Enqueue((category, logLevel));
            }
        }
    }
}