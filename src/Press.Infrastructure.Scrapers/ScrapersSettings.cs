namespace Press.Infrastructure.Scrapers;

public record ScrapersSettings
{
    public required bool UseProxy { get; init; }
    public required string ProxyAddress { get; init; }
}