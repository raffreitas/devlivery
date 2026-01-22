using Devlivery.Infrastructure.Observability.Middleware;

using Grafana.OpenTelemetry;

using Npgsql;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Devlivery.Infrastructure.Observability;

public static class ObservabilityFeature
{
    public static IServiceCollection AddObservabilityFeature(this WebApplicationBuilder builder)
    {
        const string serviceName = "devlivery-webapi";
        const string serviceNamespace = "devlivery";
        const string serviceVersion = "1.0.0";

        var services = builder.Services;

        if (string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            return services;

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            if (builder.Environment.IsProduction())
                logging.UseGrafana();
        });

        var otelBuilder = services.AddOpenTelemetry()
              .ConfigureResource(resource => resource.AddService(
                  serviceName: serviceName,
                  serviceVersion: serviceVersion,
                  serviceNamespace: serviceNamespace,
                  serviceInstanceId: Environment.MachineName
              ))
              .WithMetrics(metrics =>
              {
                  metrics.AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddProcessInstrumentation()
                      .AddNpgsqlInstrumentation()
                      .AddRuntimeInstrumentation();
              })
              .WithTracing(tracing =>
              {
                  tracing.AddSource(builder.Environment.ApplicationName)
                      .AddAspNetCoreInstrumentation(options =>
                      {
                          options.Filter = context =>
                              !context.Request.Path.StartsWithSegments("/health") &&
                              !context.Request.Path.StartsWithSegments("/alive") &&
                              !context.Request.Path.StartsWithSegments("/scalar") &&
                              !context.Request.Path.StartsWithSegments("/openapi");

                          options.RecordException = true;
                      })
                      .AddHttpClientInstrumentation()
                      .AddNpgsql();
              });

        if (builder.Environment.IsProduction())
        {
            otelBuilder.UseGrafana();
        }
        else
        {
            otelBuilder.UseOtlpExporter();
        }

        return services;
    }

    public static IApplicationBuilder UseObservabilityFeature(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}