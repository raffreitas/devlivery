using Microsoft.OpenApi;

using Scalar.AspNetCore;

namespace Devlivery.Infrastructure.Http;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira seu token JWT. Exemplo: Bearer {seu_token}"
                };

                document.Security?.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer")
                        {
                            Reference = new JsonSchemaReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                            }
                        },
                        []
                    }
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseOpenApiConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();

        return app;
    }
}