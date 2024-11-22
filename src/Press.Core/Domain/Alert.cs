namespace Press.Core.Domain;

public class Alert
{
    public string Id { get; set; }
    public string Term { get; set; }
    public string UserId { get; set; }
    public DateTime? LastNotification { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}