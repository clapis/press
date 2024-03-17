using System.Runtime.CompilerServices;
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
    IEnumerable<IPublicationProvider> providers,
    IEnumerable<IPdfContentExtractor> extractors,
    ILogger<SourcesScrapeHandler> logger)
    : IRequestHandler<SourcesScrapeRequest>
{
    public async Task Handle(SourcesScrapeRequest request, CancellationToken cancellationToken)
    {
        // Get all stored publication urls, so we just download new publications
        var urls = await store.GetAllUrlsAsync(cancellationToken);
        var stored = new HashSet<string>(urls, StringComparer.OrdinalIgnoreCase);

        // Scrape sources for publications
        await foreach (var publication in GetPublicationsAsync(cancellationToken))
        {
            // Exclude publications we already have downloaded
            if (stored.Contains(publication.Url))
                continue;

            // Extract contents
            var contents = await TryExtractContentsAsync(publication, cancellationToken);

            if (!string.IsNullOrEmpty(contents))
            {
                publication.Contents = contents;

                // Store the publication
                await store.SaveAsync(publication, cancellationToken);

                logger.LogInformation("New publication scraped {Url}", publication.Url);
            }
        }
    }

    private async IAsyncEnumerable<Publication> GetPublicationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var provider in providers)
        await foreach (var publication in provider.ProvideAsync(cancellationToken))
            yield return publication;
    }

    private async Task<string?> TryExtractContentsAsync(Publication publication, CancellationToken token)
    {
        try
        {
            var builder = new StringBuilder();

            builder.AppendLine(publication.Source.ToString());

            var extractions = await Task.WhenAll(extractors
                .Select(extractor => extractor.ExtractAsync(publication.Url, token)));

            builder.AppendJoin(" ", extractions);

            return builder.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract content from publication {Url}", publication.Url);

            return default;
        }
    }
}