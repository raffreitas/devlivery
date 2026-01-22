using System.Text.Json.Serialization;

using Devlivery.Common.Mediator.Behaviors;
using Devlivery.Features.Auth;
using Devlivery.Features.CashRegister;
using Devlivery.Features.Dashboard;
using Devlivery.Features.Expenses;
using Devlivery.Features.Orders;
using Devlivery.Features.Products;
using Devlivery.Infrastructure.Authorization;
using Devlivery.Infrastructure.Http;
using Devlivery.Infrastructure.Http.Configuration;
using Devlivery.Infrastructure.Http.ExceptionHandlers;
using Devlivery.Infrastructure.Identity;
using Devlivery.Infrastructure.Observability;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Time;
using Devlivery.Infrastructure.Time.Abstractions;

using FluentValidation;

namespace Devlivery;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddValidatorsFromAssembly(typeof(Startup).Assembly);

        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(DomainEventTenantBehavior<,>),
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