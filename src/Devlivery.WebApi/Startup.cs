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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Configurar Forwarded Headers para funcionar atrás de proxy (Nginx/Railway)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
                                     | ForwardedHeaders.XForwardedProto
                                     | ForwardedHeaders.XForwardedHost;
            
            // Em produção, confiar apenas na rede privada do Railway
            if (!builder.Environment.IsDevelopment())
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            }
            
            // Limite de proxies para evitar spoofing
            options.ForwardLimit = 2;
        });

        // Validators
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecksConfiguration();
        
        // Configurar HSTS para produção
        if (!builder.Environment.IsDevelopment())
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
        }

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

        // Security Headers & HTTPS
        if (app.Environment.IsDevelopment())
        {
            // Em desenvolvimento, redireciona para HTTPS
            app.UseHttpsRedirection();
        }
        else
        {
            // Em produção (Railway), usa HSTS mas NÃO redireciona
            // O Railway/Nginx já fazem SSL termination
            app.UseHsts();
            
            // Configura para confiar nos headers do proxy (X-Forwarded-*)
            app.UseForwardedHeaders();
            
            // Adiciona headers de segurança adicionais
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
                await next();
            });
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