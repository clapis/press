using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Press.Hosts.WebAPI.OpenTelemetry;

public static class GrafanaCloudExtensions
{
    private const string PathExtensionLogs = "logs";
    private const string PathExtensionTraces = "traces";
    private const string PathExtensionMetrics = "metrics";

    public static IServiceCollection ConfigureGrafanaCloudExporter(this IServiceCollection services, GrafanaCloudOptions grafana)
    {
        services
            .Configure<OtlpExporterOptions>(options => 
            {
                options.Endpoint = grafana.Endpoint;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                options.Headers = BuildAuthorizationHeader(grafana);
            });

        return services;
    }
    
    public static OpenTelemetryLoggerOptions AddGrafanaCloudExporter(this OpenTelemetryLoggerOptions options)
        => options.AddOtlpExporter(opts
            => opts.Endpoint = new Uri(opts.Endpoint, PathExtensionLogs));

    public static MeterProviderBuilder AddGrafanaCloudExporter(this MeterProviderBuilder builder) 
        => builder.AddOtlpExporter(opts 
            => opts.Endpoint = new Uri(opts.Endpoint, PathExtensionMetrics));

    public static TracerProviderBuilder AddGrafanaCloudExporter(this TracerProviderBuilder builder)
        => builder.AddOtlpExporter(opts
            => opts.Endpoint = new Uri(opts.Endpoint, PathExtensionTraces));

    private static string BuildAuthorizationHeader(GrafanaCloudOptions options) => 
        $"Authorization=Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{options.InstanceId}:{options.ApiKey}"))}";
}