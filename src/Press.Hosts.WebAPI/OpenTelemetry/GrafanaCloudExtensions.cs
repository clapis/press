using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Press.Hosts.WebAPI.OpenTelemetry;

public static class GrafanaCloudExtensions
{
    private const string PathExtensionTraces = "traces";
    private const string PathExtensionMetrics = "metrics";

    public static OpenTelemetryBuilder UseGrafanaCloudExporter(this OpenTelemetryBuilder builder, IConfiguration configuration)
    {
        builder.Services.Configure<GrafanaCloudOptions>(configuration);
        
        builder.Services
            .AddOptions<OtlpExporterOptions>()
            .Configure<IOptions<GrafanaCloudOptions>>((exporter, grafana) =>
            {
                exporter.Endpoint = grafana.Value.Endpoint;
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                exporter.Headers = BuildAuthorizationHeader(grafana.Value);
            });
        
        builder
            .WithTracing(x => x.AddGrafanaCloudExporter())
            .WithMetrics(x => x.AddGrafanaCloudExporter());

        return builder;
    }

    private static MeterProviderBuilder AddGrafanaCloudExporter(this MeterProviderBuilder builder) 
        => builder.AddOtlpExporter(opts 
            => opts.Endpoint = new Uri(opts.Endpoint, PathExtensionMetrics));

    private static TracerProviderBuilder AddGrafanaCloudExporter(this TracerProviderBuilder builder)
        => builder.AddOtlpExporter(opts
            => opts.Endpoint = new Uri(opts.Endpoint, PathExtensionTraces));

    private static string BuildAuthorizationHeader(GrafanaCloudOptions options) => 
        $"Authorization=Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{options.InstanceId}:{options.ApiKey}"))}";
}