using Asp.Versioning;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Common.Endpoints;

[Trait("Category", "Unit Tests")]
public sealed class EndpointsRegistrationTests
{
    // Route table of the API: changing a route or tag here is a breaking change for the web app.
    private static readonly string[] ExpectedRoutes =
    [
        "DELETE /api/v{version:apiVersion}/expenses/categories/{categoryId:guid} [Expenses]",
        "DELETE /api/v{version:apiVersion}/expenses/{expenseId:guid} [Expenses]",
        "DELETE /api/v{version:apiVersion}/orders/{id:guid} [Orders]",
        "DELETE /api/v{version:apiVersion}/products/{id:guid} [Products]",
        "GET /api/v{version:apiVersion}/cash-register/sessions [CashRegister]",
        "GET /api/v{version:apiVersion}/cash-register/sessions/active [CashRegister]",
        "GET /api/v{version:apiVersion}/cash-register/sessions/{cashSessionId:guid}/deposits [CashRegister]",
        "GET /api/v{version:apiVersion}/cash-register/sessions/{id:guid} [CashRegister]",
        "GET /api/v{version:apiVersion}/dashboard/expense-summary [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/expenses-by-category [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/expenses-by-status [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/expenses-over-time [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/orders-by-status [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/payment-breakdown [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/sales-over-time [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/stats [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/top-products [Dashboard]",
        "GET /api/v{version:apiVersion}/dashboard/upcoming-expenses [Dashboard]",
        "GET /api/v{version:apiVersion}/expenses [Expenses]",
        "GET /api/v{version:apiVersion}/expenses/categories [Expenses]",
        "GET /api/v{version:apiVersion}/expenses/{expenseId:guid} [Expenses]",
        "GET /api/v{version:apiVersion}/orders [Orders]",
        "GET /api/v{version:apiVersion}/orders/{id:guid} [Orders]",
        "GET /api/v{version:apiVersion}/products [Products]",
        "GET /api/v{version:apiVersion}/products/{id:guid} [Products]",
        "PATCH /api/v{version:apiVersion}/cash-register/sessions/{id:guid}/close [CashRegister]",
        "PATCH /api/v{version:apiVersion}/expenses/{expenseId:guid}/mark-as-paid [Expenses]",
        "PATCH /api/v{version:apiVersion}/orders/{id:guid}/status [Orders]",
        "POST /api/v{version:apiVersion}/auth/login [Auth]",
        "POST /api/v{version:apiVersion}/cash-register/sessions [CashRegister]",
        "POST /api/v{version:apiVersion}/cash-register/sessions/{cashSessionId:guid}/deposits [CashRegister]",
        "POST /api/v{version:apiVersion}/expenses [Expenses]",
        "POST /api/v{version:apiVersion}/expenses/categories [Expenses]",
        "POST /api/v{version:apiVersion}/orders [Orders]",
        "POST /api/v{version:apiVersion}/products [Products]",
        "PUT /api/v{version:apiVersion}/expenses/categories/{categoryId:guid} [Expenses]",
        "PUT /api/v{version:apiVersion}/expenses/{expenseId:guid} [Expenses]",
        "PUT /api/v{version:apiVersion}/orders/{id:guid} [Orders]",
        "PUT /api/v{version:apiVersion}/products/{id:guid} [Products]",
    ];

    [Fact]
    public async Task GivenRegisteredEndpoints_WhenMapping_ThenExposesExpectedRoutesAndTags()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApiConfiguration();
        builder.Services.AddHandlers();
        builder.Services.AddEndpoints();
        builder.Services.AddRateLimiter(_ => { });
        await using var app = builder.Build();

        // Act
        app.MapEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint =>
            {
                var version = endpoint.Metadata.GetRequiredMetadata<ApiVersionMetadata>();
                var method = endpoint.Metadata.GetRequiredMetadata<HttpMethodMetadata>().HttpMethods.Single();
                var tags = endpoint.Metadata.OfType<ITagsMetadata>().SelectMany(t => t.Tags);
                version.Map(ApiVersionMapping.Explicit).SupportedApiVersions
                    .ShouldBe([new ApiVersion(1, 0)]);
                return $"{method} {endpoint.RoutePattern.RawText} [{string.Join(",", tags)}]";
            });

        routes.ShouldBe(ExpectedRoutes, ignoreOrder: true);
    }

    [Fact]
    public void GivenApiAssembly_WhenAddingEndpoints_ThenRegistersEveryEndpointClass()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpointTypes = typeof(IEndpoint).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .ToList();

        // Act
        services.AddEndpoints();

        // Assert
        endpointTypes.Count.ShouldBe(ExpectedRoutes.Length);
        services.Where(d => d.ServiceType == typeof(IEndpoint)).Select(d => d.ImplementationType)
            .ShouldBe(endpointTypes, ignoreOrder: true);
    }
}
