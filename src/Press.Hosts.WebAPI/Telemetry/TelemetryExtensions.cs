using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Press.Hosts.WebAPI.Telemetry;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, TelemetryOptions options)
    {
        if (!options.IsEnabled) return services;

        services
            .Configure<OtlpExporterOptions>(opts => opts.Endpoint = options.Endpoint);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracer => tracer
                .AddInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(meter => meter
                .AddInstrumentation()
                .AddOtlpExporter());

        services.AddLogging(opts => opts
            .ClearProviders()
            .AddConsole()
            .AddOpenTelemetry(log => log.AddOtlpExporter()));

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
        .AddHttpClientInstrumentation(opts =>
        {
            opts.EnrichWithHttpRequestMessage = (activity, message) =>
            {
                activity.SetTag("http.request.uri", message.RequestUri);
                activity.SetTag("http.request.user_agent", message.Headers.UserAgent);
            };
        })
        .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
}