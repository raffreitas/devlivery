using Devlivery.WebApi.Features.Auth;
using Devlivery.WebApi.Features.Dashboard;
using Devlivery.WebApi.Features.Orders;
using Devlivery.WebApi.Features.Products;
using Devlivery.WebApi.Shared.Database;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Database.Seeder;
using Devlivery.WebApi.Shared.Identity;
using Devlivery.WebApi.Shared.Identity.Models;
using Devlivery.WebApi.Shared.Observability;
using Devlivery.WebApi.Shared.Presentation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Validators
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecksConfiguration();

        // OpenAPI/Swagger
        services.AddOpenApiConfiguration();

        // Shared Infrastructure
        services.AddIdentityFeature(configuration);
        services.AddDatabaseFeature(configuration);

        // Features
        services.AddAuthFeature(configuration);
        services.AddOrderFeature();
        services.AddProductFeature();

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

        // CORS
        app.UseCorsConfiguration();

        // Security
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseObservabilityFeature();

        // Endpoints
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
        app.MapDashboardEndpoints();
    }
}