using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Press.Hosts.WebAPI.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder ConfigureInstrumentation(this OpenTelemetryBuilder services) => services
        .ConfigureResource(resource => resource.AddService("press"))
        .WithMetrics(meter => meter.AddInstrumentation())
        .WithTracing(tracer => tracer.AddInstrumentation());

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