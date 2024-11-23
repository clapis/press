namespace Press.Core.Domain;

public class Source
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required bool IsEnabled { get; init; }
    public required bool IsOfficial { get; init; }
}