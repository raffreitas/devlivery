using Grafana.OpenTelemetry;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Devlivery.BackupJob.Observability;

public static class BackupObservabilityFeature
{
    public static IServiceCollection AddBackupObservabilityFeature(this HostApplicationBuilder builder)
    {
        var services = builder.Services;

        if (string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            return services;
        }

        services.Configure<OpenTelemetryLoggerOptions>(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: builder.Environment.ApplicationName,
                serviceInstanceId: Environment.MachineName
            ))
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(BackupTelemetry.MeterName)
                    .AddHttpClientInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(BackupTelemetry.ActivitySourceName)
                    .AddHttpClientInstrumentation();
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
}