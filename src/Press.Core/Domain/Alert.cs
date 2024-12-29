namespace Press.Core.Domain;

public class Alert
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Keyword { get; set; }
    public string SourceId { get; set; }
    public DateTime? LastNotification { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}