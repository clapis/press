namespace Press.Core.Infrastructure.Scrapers;

public interface IPdfContentExtractor
{
    Task<string> ExtractAsync(string url, CancellationToken cancellationToken);
    Task<string> ExtractTextAsync(string filepath, CancellationToken cancellationToken);
}