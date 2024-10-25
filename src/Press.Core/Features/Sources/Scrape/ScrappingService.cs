using Microsoft.Extensions.Logging;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Sources.Scrape;

public class ScrappingService(
    IPublicationStore store,
    IPublicationProvider provider, 
    IPdfContentExtractor extractor,
    ILogger<ScrappingService> logger)
{
    public async Task ScrapeAsync(Source source, CancellationToken cancellationToken)
    {
        // Retrieve latest publication urls, so that we process only what's new
        var urls = await store.GetLatestUrlsAsync(source, cancellationToken);
        var stored = new HashSet<string>(urls, StringComparer.OrdinalIgnoreCase);

        // Scrape source for latest publications
        var publications = await provider.ProvideAsync(source, cancellationToken);

        // Extract contents of new publications
        foreach (var publication in publications.Where(publication => !stored.Contains(publication.Url)))
        {
            var contents = await extractor.ExtractAsync(publication.Url, cancellationToken);

            publication.Contents = contents;

            await store.SaveAsync(publication, cancellationToken);

            logger.LogInformation("New publication scraped {Url}", publication.Url);
            
            if (string.IsNullOrWhiteSpace(contents))
                logger.LogWarning("Publication has no contents {Url}", publication.Url);
        }
    }}