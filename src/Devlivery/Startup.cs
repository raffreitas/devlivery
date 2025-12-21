using System.Text.Json.Serialization;

using Devlivery.Features.Auth;
using Devlivery.Features.CashRegister;
using Devlivery.Features.Expenses;
using Devlivery.Features.Orders;
using Devlivery.Features.Products;
using Devlivery.Shared.Infrastructure.Authorization;
using Devlivery.Shared.Infrastructure.Identity;
using Devlivery.Shared.Infrastructure.Identity.Users.Models;
using Devlivery.Shared.Infrastructure.Observability;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Shared.Infrastructure.Persistence.Seeder;
using Devlivery.Shared.Infrastructure.Tenancy;
using Devlivery.Shared.Infrastructure.WebServer;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Validators
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

        // CORS
        services.AddCorsConfiguration();

        // Observability
        builder.AddObservabilityFeature();
    }

    public static void ConfigureApp(WebApplication app)
    {
        app.UseOpenApiConfiguration();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            db.Database.Migrate();
            DatabaseSeeder.SeedAsync(db, userManager).GetAwaiter().GetResult();
        }

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
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
        app.MapCashRegisterEndpoints();
        app.MapExpenseEndpoints();
    }
}