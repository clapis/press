namespace Press.Core.Domain;

public class Source
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public bool IsEnabled { get; set; }
}