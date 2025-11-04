using Devlivery.WebApi.Features.Auth;
using Devlivery.WebApi.Features.Dashboard;
using Devlivery.WebApi.Features.Orders;
using Devlivery.WebApi.Features.Products;
using Devlivery.WebApi.Shared.Infrastructure.Database;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Devlivery.WebApi.Shared.Infrastructure.Database.Seeder;
using Devlivery.WebApi.Shared.Infrastructure.Identity;
using Devlivery.WebApi.Shared.Infrastructure.Identity.Models;
using Devlivery.WebApi.Shared.Infrastructure.Tokens;
using Devlivery.WebApi.Shared.Presentation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace Devlivery.WebApi;

public static class Startup
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Validators
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // OpenAPI
        builder.Services.AddOpenApi();

        // Shared Infrastructure
        builder.Services.AddIdentityFeature(builder.Configuration);
        builder.Services.AddDatabaseFeature(builder.Configuration);
        builder.Services.AddTokensFeature(builder.Configuration);

        // Application Features
        builder.Services.AddAuthFeature();

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigureApp(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            // Auto migrate and seed
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            db.Database.Migrate();
            DatabaseSeeder.SeedAsync(db, userManager).GetAwaiter().GetResult();
        }

        app.UseCors();

        // Map endpoints
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
        app.MapDashboardEndpoints();
    }
}