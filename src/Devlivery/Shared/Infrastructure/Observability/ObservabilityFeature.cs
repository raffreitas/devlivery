using Devlivery.Shared.Infrastructure.Observability.Middleware;

using Grafana.OpenTelemetry;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Devlivery.Shared.Infrastructure.Observability;

public static class ObservabilityFeature
{
    public static IServiceCollection AddObservabilityFeature(this WebApplicationBuilder builder)
    {
        const string serviceName = "devlivery-webapi";
        const string serviceNamespace = "devlivery";
        const string serviceVersion = "1.0.0";

        var services = builder.Services;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceNamespace: serviceNamespace,
                serviceInstanceId: Environment.MachineName
            ))
            .WithTracing(tracingBuilder => tracingBuilder
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health") &&
                        !context.Request.Path.StartsWithSegments("/scalar") &&
                        !context.Request.Path.StartsWithSegments("/openapi");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation())
            .WithMetrics(metricsBuilder => metricsBuilder
                .AddMeter(serviceName)
                .AddMeter()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .UseGrafana();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.UseGrafana();
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.IncludeFormattedMessage = true;
        });

        return services;
    }

    public static IApplicationBuilder UseObservabilityFeature(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}