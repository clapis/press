namespace Press.Core.Features.Sources.Scrape.Extractors;

public interface IPdfContentExtractor
{
    Task<string> ExtractAsync(string url, CancellationToken cancellationToken);
}