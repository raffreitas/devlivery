using System.Text.Json.Serialization;

using Devlivery.Features.Auth;
using Devlivery.Features.CashRegister;
using Devlivery.Features.Dashboard;
using Devlivery.Features.Expenses;
using Devlivery.Features.Orders;
using Devlivery.Features.Products;
using Devlivery.Shared.Application.Abstractions;
using Devlivery.Shared.Infrastructure.Authorization;
using Devlivery.Shared.Infrastructure.Identity;
using Devlivery.Shared.Infrastructure.Networking;
using Devlivery.Shared.Infrastructure.Observability;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using Devlivery.Shared.Infrastructure.Time;
using Devlivery.Shared.Infrastructure.WebServer;

using FluentValidation;

namespace Devlivery;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.Services.AddNetworkingFeature();

        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddValidatorsFromAssembly(typeof(Startup).Assembly);

        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(Shared.Infrastructure.Tenancy.Behaviors.DomainEventTenantBehavior<,>),
                typeof(Shared.Application.Behaviors.ValidationPipelineBehavior<,>)
            ];
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecksConfiguration();
        services.AddHttpContextAccessor();

        services.AddSingleton<IDateTimeProvider, BrazilDateTimeProvider>();

        // OpenAPI/Swagger
        services.AddOpenApiConfiguration();

        // Shared Features
        services.AddIdentityFeature(configuration);
        services.AddDatabaseFeature(configuration);
        services.AddAuthorizationFeature();
        services.AddTenancyFeature();

        // Features
        services.AddAuthFeature();
        services.AddOrderFeature();
        services.AddProductFeature();
        services.AddCashRegisterFeature();
        services.AddExpenseFeature();
        services.AddDashboardFeature();

        // CORS
        services.AddCorsConfiguration();

        // Observability
        builder.AddObservabilityFeature();
    }

    public static void ConfigureApp(WebApplication app)
    {
        app.UseOpenApiConfiguration();

        app.UseDatabaseFeature();

        app.UseExceptionHandler();

        // CORS
        app.UseCorsConfiguration();

        // Security
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthorizationFeature();
        app.UseTenancyFeature();
        app.UseObservabilityFeature();

        // Endpoints
        app.MapHealthCheckEndpoints();
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
        app.MapCashRegisterEndpoints();
        app.MapExpenseEndpoints();
        app.MapDashboardEndpoints();
    }
}