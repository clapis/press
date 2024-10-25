using MediatR;
using Microsoft.Extensions.Logging;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Sources.Scrape;

public record ScrapeSourcesRequest : IRequest;
public class ScrapeSourcesHandler(
    ISourceStore store,
    ScrappingService service,
    ILogger<ScrapeSourcesHandler> logger)
    : IRequestHandler<ScrapeSourcesRequest>
{
    public async Task Handle(ScrapeSourcesRequest request, CancellationToken cancellationToken)
    {
        var sources = await store.GetAllAsync(cancellationToken);

        await Task.WhenAll(sources
            .Where(x => x.IsEnabled)
            .Select(source => service.ScrapeAsync(source, cancellationToken)));
    }
}