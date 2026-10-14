using System.Text.Json.Serialization;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Features.CashRegister;
using Devlivery.Features.Expenses;
using Devlivery.Features.Orders;
using Devlivery.Features.Products;
using Devlivery.Infrastructure.Authorization;
using Devlivery.Infrastructure.Http.Configuration;
using Devlivery.Infrastructure.Http.ExceptionHandlers;
using Devlivery.Infrastructure.Identity;
using Devlivery.Infrastructure.Observability;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection.Extensions;

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

        services.AddHandlers();
        services.AddEndpoints();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecksConfiguration();
        services.AddHttpContextAccessor();

        services.TryAddSingleton(TimeProvider.System);

        // OpenAPI/Swagger
        services.AddOpenApiConfiguration();

        // Shared Features
        services.AddIdentityFeature(configuration);
        services.AddDatabaseFeature(configuration);
        services.AddAuthorizationFeature();
        services.AddTenancyFeature();

        // Features
        services.AddOrderFeature();
        services.AddProductFeature();
        services.AddCashRegisterFeature();
        services.AddExpenseFeature();

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
        app.UseRouting();

        // CORS
        app.UseCorsConfiguration();
        app.UseRateLimiter();

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
        app.MapEndpoints();
    }
}