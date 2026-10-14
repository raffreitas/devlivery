using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;

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
        "DELETE /api/expenses/categories/{categoryId:guid} [Expenses]",
        "DELETE /api/expenses/{expenseId:guid} [Expenses]",
        "DELETE /api/orders/{id:guid} [Orders]",
        "DELETE /api/products/{id:guid} [Products]",
        "GET /api/cash-register/sessions [CashRegister]",
        "GET /api/cash-register/sessions/active [CashRegister]",
        "GET /api/cash-register/sessions/{cashSessionId:guid}/deposits [CashRegister]",
        "GET /api/cash-register/sessions/{id:guid} [CashRegister]",
        "GET /api/dashboard/expense-summary [Dashboard]",
        "GET /api/dashboard/expenses-by-category [Dashboard]",
        "GET /api/dashboard/expenses-by-status [Dashboard]",
        "GET /api/dashboard/expenses-over-time [Dashboard]",
        "GET /api/dashboard/orders-by-status [Dashboard]",
        "GET /api/dashboard/payment-breakdown [Dashboard]",
        "GET /api/dashboard/sales-over-time [Dashboard]",
        "GET /api/dashboard/stats [Dashboard]",
        "GET /api/dashboard/top-products [Dashboard]",
        "GET /api/dashboard/upcoming-expenses [Dashboard]",
        "GET /api/expenses [Expenses]",
        "GET /api/expenses/categories [Expenses]",
        "GET /api/expenses/{expenseId:guid} [Expenses]",
        "GET /api/orders [Orders]",
        "GET /api/orders/{id:guid} [Orders]",
        "GET /api/products [Products]",
        "GET /api/products/{id:guid} [Products]",
        "PATCH /api/cash-register/sessions/{id:guid}/close [CashRegister]",
        "PATCH /api/expenses/{expenseId:guid}/mark-as-paid [Expenses]",
        "PATCH /api/orders/{id:guid}/status [Orders]",
        "POST /api/auth/login [Auth]",
        "POST /api/cash-register/sessions [CashRegister]",
        "POST /api/cash-register/sessions/{cashSessionId:guid}/deposits [CashRegister]",
        "POST /api/expenses [Expenses]",
        "POST /api/expenses/categories [Expenses]",
        "POST /api/orders [Orders]",
        "POST /api/products [Products]",
        "PUT /api/expenses/categories/{categoryId:guid} [Expenses]",
        "PUT /api/expenses/{expenseId:guid} [Expenses]",
        "PUT /api/orders/{id:guid} [Orders]",
        "PUT /api/products/{id:guid} [Products]",
    ];

    [Fact]
    public async Task GivenRegisteredEndpoints_WhenMapping_ThenExposesExpectedRoutesAndTags()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
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
                var method = endpoint.Metadata.GetRequiredMetadata<HttpMethodMetadata>().HttpMethods.Single();
                var tags = endpoint.Metadata.OfType<ITagsMetadata>().SelectMany(t => t.Tags);
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