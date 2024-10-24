namespace Press.Core.Domain;

public class Publication
{
    public string Id { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
    public string Contents { get; set; }
    public string SourceId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}