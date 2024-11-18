using Microsoft.Extensions.Logging;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Core.Features.Sources.Scrape;

public class ScrapingService(
    IPublicationStore store,
    IPublicationProvider provider, 
    IPdfContentExtractor extractor,
    ILogger<ScrapingService> logger)
{
    public async Task ScrapeAsync(Source source, CancellationToken cancellationToken)
    {
        // Scrape source for latest publications
        var publications = await provider.ProvideAsync(source, cancellationToken);
        
        // Retrieve latest publication already previously scraped
        var stored = await store.GetLatestUrlsAsync(source, cancellationToken);

        // Extract contents of new publications only
        foreach (var publication in publications.ExceptBy(stored, x => x.Url))
        {
            publication.Contents = await extractor.ExtractAsync(publication.Url, cancellationToken);

            await store.SaveAsync(publication, cancellationToken);

            logger.LogInformation("New publication scraped {Url}", publication.Url);
            
            if (string.IsNullOrWhiteSpace(publication.Contents))
                logger.LogWarning("Publication has no contents {Url}", publication.Url);
        }
    }}