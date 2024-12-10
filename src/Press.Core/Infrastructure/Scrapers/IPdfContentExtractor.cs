namespace Press.Core.Infrastructure.Scrapers;

public interface IPdfContentExtractor
{
    Task<string> ExtractTextAsync(string filepath, CancellationToken cancellationToken);
}