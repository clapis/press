using System.Text.Json;
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
        .AddHttpClientInstrumentation(opts =>
        {
            opts.EnrichWithHttpRequestMessage = (activity, message) =>
            {
                activity.SetTag("http.request.uri", message.RequestUri);
                activity.SetTag("http.request.user_agent", message.Headers.UserAgent);
            };
            
            opts.EnrichWithHttpResponseMessage = (activity, message) =>
            {
                activity.SetTag("http.response.headers.location", message.Headers.Location);
            }; 

        })
        .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
}