using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Core.Features.Sources.Scrape;

public record ScrapeSourcesRequest : IRequest
{
    public string[] Ids { get; init; } = [];
}

public class ScrapeSourcesHandler(
    ISourceStore sourceStore,
    IPublicationStore publicationStore,
    IEnumerable<IPublicationScraper> providers,
    ILogger<ScrapeSourcesHandler> logger)
    : IRequestHandler<ScrapeSourcesRequest>
{
    
    public async Task Handle(ScrapeSourcesRequest request, CancellationToken cancellationToken)
    {
        var sources = await sourceStore.GetAllAsync(cancellationToken);

        if (request.Ids.Any())
            sources = sources.IntersectBy(request.Ids, x => x.Id).ToList();

        // do this sequentially - we're mindful of memory and not in a rush
        foreach (var source in sources)
            await ScrapeSourceAsync(source, cancellationToken);
    }

    private async Task ScrapeSourceAsync(Source source, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var publications = 0;

        try
        {
            if (!source.IsEnabled)
            {
                logger.LogInformation("Scraping: {Source} is disabled, skipping.", source.Name);
                return;
            }

            var provider = GetPublicationScraper(source);
            
            var existing = await publicationStore.GetLatestUrlsAsync(source, cancellationToken);
            
            await foreach (var publication in provider.ScrapeAsync(existing, cancellationToken))
            {
                await publicationStore.SaveAsync(publication, cancellationToken);
                publications++;
            }

            logger.LogInformation("Scraping: {Source} completed. ({Publications} docs, {Duration} ms)", source.Name, publications, sw.ElapsedMilliseconds);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scraping: {Source} failed. ({Publications} docs, {Duration} ms)", source.Name, publications, sw.ElapsedMilliseconds);
        }
    }
    
    private IPublicationScraper GetPublicationScraper(Source source)
    {
        var results =  providers.Where(x => x.SourceId == source.Id).ToList();

        if (results == null)
            throw new Exception($"Failed to find the scraper for source '{source.Id}'");
        
        if (results.Count > 1)
            throw new Exception($"There are multiple scrapers registered with source '{source.Id}'");

        return results.Single();
    }

}