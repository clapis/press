namespace Press.Core.Domain;

public class Subscription
{
    public required string Name { get; set; }
    public required int MaxAlerts { get; set; }
    public bool IsTrial { get; set; }
}