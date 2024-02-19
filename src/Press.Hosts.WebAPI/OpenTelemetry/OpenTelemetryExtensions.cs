using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Press.Hosts.WebAPI.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, GrafanaCloudOptions options)
    {
        services
            .ConfigureGrafanaCloudExporter(options);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("press"))
            .WithTracing(tracer => tracer
                .AddInstrumentation()
                .AddGrafanaCloudExporter())
            .WithMetrics(meter => meter
                .AddInstrumentation()
                .AddGrafanaCloudExporter());
        
        services
            .AddLogging(x => x.AddOpenTelemetry(logger => logger
                .AddGrafanaCloudExporter()));

        return services;
    }
    
    private static MeterProviderBuilder AddInstrumentation(this MeterProviderBuilder builder) => builder
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();

    private static TracerProviderBuilder AddInstrumentation(this TracerProviderBuilder builder) => builder
        .AddQuartzInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
}