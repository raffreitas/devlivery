using Devlivery.WebApi.Features.Auth;
using Devlivery.WebApi.Features.Dashboard;
using Devlivery.WebApi.Features.Orders;
using Devlivery.WebApi.Features.Products;
using Devlivery.WebApi.Shared.Infrastructure.Database;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Devlivery.WebApi.Shared.Infrastructure.Database.Seeder;
using Devlivery.WebApi.Shared.Infrastructure.Identity;
using Devlivery.WebApi.Shared.Infrastructure.Identity.Models;
using Devlivery.WebApi.Shared.Presentation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
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

        // Reverse proxy headers (Railway, Docker, etc.)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Trust all proxies/networks by clearing the known lists; rely on platform firewall
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    public static void ConfigureApp(WebApplication app)
    {
        app.UseOpenApiConfiguration();

        // Root endpoint for platform health checks (Railway)
        app.MapGet("/", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            db.Database.Migrate();
            DatabaseSeeder.SeedAsync(db, userManager).GetAwaiter().GetResult();
        }

        // Forward proxy headers early so scheme/host are correct behind Railway
        app.UseForwardedHeaders();

        // CORS
        app.UseCorsConfiguration();

        // Security
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // In PaaS (Railway) HTTPS is terminated at the edge; avoid redirect loops/port issues
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoints
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
        app.MapDashboardEndpoints();
    }
}