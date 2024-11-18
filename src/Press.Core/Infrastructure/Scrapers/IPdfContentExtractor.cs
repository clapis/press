namespace Press.Core.Infrastructure.Scrapers;

public interface IPdfContentExtractor
{
    Task<string> ExtractAsync(string url, CancellationToken cancellationToken);
}