using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Sources.Scrape;

public record ScrapeSourcesRequest : IRequest
{
    public string[] Ids { get; init; } = [];
}

public class ScrapeSourcesHandler(
    ISourceStore store,
    ScrapingService service,
    ILogger<ScrapeSourcesHandler> logger)
    : IRequestHandler<ScrapeSourcesRequest>
{
    
    public async Task Handle(ScrapeSourcesRequest request, CancellationToken cancellationToken)
    {
        var sources = await store.GetAllAsync(cancellationToken);

        if (request.Ids.Any())
            sources = sources.IntersectBy(request.Ids, x => x.Id).ToList();

        // do this sequentially - we're mindful of memory and not in a rush
        foreach (var source in sources)
            await ScrapeSourceAsync(source, cancellationToken);
    }

    private async Task ScrapeSourceAsync(Source source, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            if (!source.IsEnabled)
            {
                logger.LogInformation("Scraping: {Source} is disabled, skipping.", source.Name);
                return;
            }

            await service.ScrapeAsync(source, cancellationToken);
            
            logger.LogInformation("Scraping: {Source} completed after {Duration} ms", source.Name, sw.ElapsedMilliseconds);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scraping: {Source} failed after {Duration} ms", source.Name, sw.ElapsedMilliseconds);
        }
    }
}