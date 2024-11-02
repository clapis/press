namespace Press.Hosts.WebAPI.Telemetry;

public record TelemetryOptions 
{
    public required Uri Endpoint { get; init; }
    public required bool IsEnabled { get; init; }
    public required string ServiceName { get; init; }
}