using Press.Core.Domain;

namespace Press.Core.Infrastructure.Scrapers;

public interface IPublicationScraper
{
    string SourceId { get; }

    IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, CancellationToken cancellationToken);
}