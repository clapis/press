namespace Press.Hosts.WebAPI.OpenTelemetry;

public class GrafanaCloudOptions 
{
    public bool IsEnabled { get; set; }
    public required string InstanceId { get; init; }
    public required string ApiKey { get; init; }
    public required Uri Endpoint { get; init; }
}