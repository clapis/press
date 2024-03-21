using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Press.Core.Domain;
using Press.Core.Features.Sources.Scrape.Extractors;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Core.Features.Sources.Scrape;

public class SourcesScrapeHandler(
    IPublicationStore store,
    IPdfContentExtractor extractor,
    IEnumerable<IPublicationProvider> providers,
    ILogger<SourcesScrapeHandler> logger)
    : IRequestHandler<SourcesScrapeRequest>
{
    public async Task Handle(SourcesScrapeRequest request, CancellationToken cancellationToken)
    {
        foreach (var source in request.Sources)
        {
            // Retrieve latest publication urls, so that we process only what's new
            var urls = await store.GetLatestUrlsAsync(source, cancellationToken);
            var stored = new HashSet<string>(urls, StringComparer.OrdinalIgnoreCase);

            // Scrape source for latest publication links
            var provider = providers.Single(x => x.Source == source);
            var publications = await provider.ProvideAsync(cancellationToken);

            // Extract contents of new publications
            foreach (var publication in publications.Where(publication => !stored.Contains(publication.Url)))
            {
                var contents = await TryExtractContentsAsync(publication, cancellationToken);

                if (!string.IsNullOrEmpty(contents))
                {
                    publication.Contents = contents;

                    await store.SaveAsync(publication, cancellationToken);

                    logger.LogInformation("New publication scraped {Url}", publication.Url);
                }
            }
        }
    }

    private async Task<string?> TryExtractContentsAsync(Publication publication, CancellationToken token)
    {
        try
        {
            var contents = await extractor.ExtractAsync(publication.Url, token);

            var builder = new StringBuilder();

            builder.Append($"{publication.Source} ");
            builder.Append(contents);

            return builder.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract content from publication {Url}", publication.Url);

            return default;
        }
    }
}