using Asp.Versioning;

using Devlivery.Infrastructure.Http.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

namespace Devlivery.Infrastructure.Http.Configuration;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        })
        .AddOpenApi(options =>
        {
            options.Document.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Devlivery API";
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Obtenha o JWT no login e cole apenas o token, sem o prefixo Bearer."
                };

                return Task.CompletedTask;
            });
            options.Document.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                if (!metadata.OfType<IAllowAnonymous>().Any())
                {
                    operation.Security = [new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    }];
                }

                if (metadata.OfType<IdempotencyEndpointMetadata>().Any())
                {
                    operation.Parameters ??= [];
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = IdempotencyFilter.HeaderName,
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
                        Description = "UUID da operação, reutilizado em reenvios."
                    });
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseOpenApiConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.MapOpenApi().WithDocumentPerVersion().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Devlivery · Referência da API")
                .AddPreferredSecuritySchemes("Bearer");

            foreach (var description in app.DescribeApiVersions())
            {
                options.AddDocument(description.GroupName, description.GroupName,
                    isDefault: description.GroupName == "v1");
            }
        }).AllowAnonymous();

        return app;
    }
}
